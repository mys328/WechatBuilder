﻿using WechatBuilder.BLL;
using WechatBuilder.Common;
using WechatBuilder.WeiXinComm.CustomMessageHandler;
using Senparc.Weixin.MP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
 
namespace WechatBuilder.Web.api.weixin
{
    public partial class api : System.Web.UI.Page
    {
        
      
        protected void Page_Load(object sender, EventArgs e)
        { 
            string Token = "";//与微信公众账号后台的Token设置保持一致，区分大小写。
            int wid = 0;
            wid = MyCommFun.RequestInt("apiid");
            
            if (wid <= 0)
            {
                WriteContent("参数非法");
                return;
            }

            wx_userweixin wbll = new wx_userweixin();
            Token = wbll.GetWeiXinToken(wid);
            if (Token == null || string.IsNullOrEmpty(Token))
            {
                WriteContent("不存在该微信号！");
                return;
            }
            
           // Token = "uweixin";
            string signature = Request["signature"];
            string timestamp = Request["timestamp"];
            string nonce = Request["nonce"];
            string echostr = Request["echostr"];
            
            if (Request.HttpMethod == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent(echostr); //返回随机字符串则表示验证通过
                }
                else
                {
                    WriteContent("failed:" + signature + ",token:"+Token+" " + CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                                "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
                }
                Response.End();
            }
            else
            {
                //post method - 当有用户想公众账号发送消息时触发
                //本地测试的时候注释掉 ----start -----

                if (!CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent("参数错误！");
                    return;
                }

                //本地测试的时候注释掉 ----end -----
                //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
                var maxRecordCount = 10;

                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                var messageHandler = new CustomMessageHandler(Request.InputStream, maxRecordCount);

                try
                {
                    //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
                    //messageHandler.RequestDocument.Save(
                    //    Server.MapPath("~/App_Data/" + DateTime.Now.Ticks + "_Request_" +
                    //                   messageHandler.RequestMessage.FromUserName + ".txt"));
                    //执行微信处理过程
                    messageHandler.Execute();
                    //测试时可开启，帮助跟踪数据
                    //messageHandler.ResponseDocument.Save(
                    //    Server.MapPath("~/App_Data/" + DateTime.Now.Ticks + "_Response_" +
                    //                   messageHandler.ResponseMessage.ToUserName + ".txt"));
                    WriteContent(messageHandler.ResponseDocument.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    using (TextWriter tw = new StreamWriter(Server.MapPath("~/App_Data/Error_" + DateTime.Now.Ticks + ".txt")))
                    {
                        tw.WriteLine(ex.Message);
                        tw.WriteLine(ex.InnerException.Message);
                        if (messageHandler.ResponseDocument != null)
                        {
                            tw.WriteLine(messageHandler.ResponseDocument.ToString());
                        }
                        tw.Flush();
                        tw.Close();
                    }
                    WriteContent("");
                }
                finally
                {
                    Response.End();
                }
            }
        }

        private void WriteContent(string str)
        {
            Response.Output.Write(str);
        }

        /// <summary>
        /// 最简单的Page_Load写法（本方法仅用于演示过程，未实际使用到）
        /// </summary>
        //private void MiniProcess()
        //{
        //    string signature = Request["signature"];
        //    string timestamp = Request["timestamp"];
        //    string nonce = Request["nonce"];
        //    string echostr = Request["echostr"];

        //    if (Request.HttpMethod == "GET")
        //    {
        //        //get method - 仅在微信后台填写URL验证时触发
        //        if (CheckSignature.Check(signature, timestamp, nonce, Token))
        //        {
        //            WriteContent(echostr); //返回随机字符串则表示验证通过
        //        }
        //        else
        //        {
        //            WriteContent("failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token));
        //        }

        //    }
        //    else
        //    {
        //        //post method - 当有用户想公众账号发送消息时触发
        //        if (!CheckSignature.Check(signature, timestamp, nonce, Token))
        //        {
        //            WriteContent("参数错误！");
        //        }

        //        //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
        //        var messageHandler = new CustomMessageHandler(Request.InputStream);
        //        //执行微信处理过程
        //        messageHandler.Execute();
        //        //输出结果
        //        WriteContent(messageHandler.ResponseDocument.ToString());
        //    }
        //    Response.End();
        //}
        
    }
}