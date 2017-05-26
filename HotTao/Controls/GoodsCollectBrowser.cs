﻿using CefSharp;
using CefSharp.WinForms;
using HotTao.Properties;
using HotTaoCore.Logic;
using HotTaoCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotTao.Controls
{
    public partial class GoodsCollectBrowser : FormEx
    {

        #region 移动窗口
        /*
         * 首先将窗体的边框样式修改为None，让窗体没有标题栏
         * 实现这个效果使用了三个事件：鼠标按下、鼠标弹起、鼠标移动
         * 鼠标按下时更改变量isMouseDown标记窗体可以随鼠标的移动而移动
         * 鼠标移动时根据鼠标的移动量更改窗体的location属性，实现窗体移动
         * 鼠标弹起时更改变量isMouseDown标记窗体不可以随鼠标的移动而移动
         */
        private bool isMouseDown = false;
        private Point FormLocation;     //form的location
        private Point mouseOffset;      //鼠标的按下位置
        private void WinForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                FormLocation = this.Location;
                mouseOffset = Control.MousePosition;
            }
        }

        private void WinForm_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }
        private void WinForm_MouseMove(object sender, MouseEventArgs e)
        {
            int _x = 0;
            int _y = 0;
            if (isMouseDown)
            {
                Point pt = Control.MousePosition;
                _x = mouseOffset.X - pt.X;
                _y = mouseOffset.Y - pt.Y;

                this.Location = new Point(FormLocation.X - _x, FormLocation.Y - _y);
            }

        }
        #endregion

        private Main hotForm { get; set; }


        private string defaultUrl { get; set; }


        public GoodsCollectBrowser(Main form, string url = "")
        {
            hotForm = form;
            defaultUrl = url;
            InitializeComponent();
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

        }

        private void picClose_Click(object sender, EventArgs e)
        {
            if (!Runing)
            {
                hotForm.collectBrowser.Dispose();
                hotForm.collectBrowser = null;
                this.Close();
            }
            else
            {
                MessageConfirm confirm = new MessageConfirm("正在进行商品采集，你确定要退出吗？");
                confirm.CallBack += () =>
                {
                    hotForm.collectBrowser.Dispose();
                    hotForm.collectBrowser = null;
                    this.Close();

                };
                confirm.ShowDialog(this);
            }
        }

        private void GoodsCollectBrowser_Load(object sender, EventArgs e)
        {
            //初始化
            InitBrowser(defaultUrl);
        }

        private ChromiumWebBrowser browser { get; set; }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        public void InitBrowser(string url = "")
        {
            new System.Threading.Thread(() =>
            {
                if (browser == null)
                {
                    BrowserSettings settings = new BrowserSettings()
                    {
                        LocalStorage = CefState.Enabled,
                        Javascript = CefState.Enabled,
                        Plugins = CefState.Enabled,
                        ImageLoading = CefState.Enabled,
                        ImageShrinkStandaloneToFit = CefState.Enabled,
                        WebGl = CefState.Enabled
                    };
                    browser = new ChromiumWebBrowser(url);
                    browser.BrowserSettings = settings;
                    browser.Dock = DockStyle.Fill;
                    browser.LifeSpanHandler = new LifeSpanHandler();
                    browser.AddressChanged += Browser_AddressChanged;
                    AddBrowser();
                }
                else
                    browser.Load(url);
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 添加浏览控件到展示界面
        /// </summary>
        private void AddBrowser()
        {
            if (panelBrowser.InvokeRequired)
            {
                panelBrowser.Invoke(new Action(AddBrowser));
            }
            else
            {
                panelBrowser.Controls.Add(browser);
            }
        }

        /// <summary>
        /// 设置商品链接
        /// </summary>
        /// <param name="url"></param>
        public void SetGoodsUrl(string url)
        {
            if (this.txtAddress.InvokeRequired)
            {
                this.txtAddress.Invoke(new Action<string>(SetGoodsUrl), new object[] { url });
            }
            else
            {
                txtAddress.Text = url;
            }
        }

        /// <summary>
        /// 地址发送改变时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            SetGoodsUrl(e.Address);
        }

        private void GoodsCollectBrowser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                OpenGoodsUrl();
            }
        }

        private void picRefresh_Click(object sender, EventArgs e)
        {
            OpenGoodsUrl();
        }


        /// <summary>
        /// 打开地址
        /// </summary>
        private void OpenGoodsUrl()
        {
            if (string.IsNullOrEmpty(txtAddress.Text))
            {
                return;
            }
            LoadBrowser(txtAddress.Text);
        }

        /// <summary>
        /// 加载浏览器地址
        /// </summary>
        /// <param name="address"></param>
        public void LoadBrowser(string address)
        {
            browser.Load(address);
            browser.Focus();
            SetGoodsUrl(txtAddress.Text);
        }



        private void picForward_Click(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void picBack_Click(object sender, EventArgs e)
        {
            browser.Back();
        }


        /// <summary>
        /// 当前窗口是否已是最大化
        /// </summary>
        private bool isMax { get; set; }


        /// <summary>
        /// 最大化切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picMax_Click(object sender, EventArgs e)
        {
            if (!isMax)
            {
                this.WindowState = FormWindowState.Maximized;
                picMax.Image = Resources.max_01;
                isMax = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                picMax.Image = Resources.max;
                isMax = false;
            }
            ResizeWin();
        }

        /// <summary>
        /// 窗口大小发生变化时调用
        /// </summary>
        private void ResizeWin()
        {
            RECT rect = new RECT();
            WinApi.GetWindowRect(this.Handle, ref rect);
            plSite.Width = rect.Right - rect.Left - 195;
            panelBrowser.Size = new Size(rect.Right - rect.Left - 2, rect.Bottom - rect.Top - 94);
            plRightTop.Location = new Point(rect.Right - rect.Left - 112, 0);
            plfooter.Width = rect.Right - rect.Left;
            plfooter.Location = new Point(0, rect.Bottom - rect.Top - 60);
            btnAddGoods.Location = new Point(rect.Right - rect.Left - 100, btnAddGoods.Location.Y);

        }


        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        /// <summary>
        /// 是否在执行
        /// </summary>
        private bool Runing { get; set; }
        /// <summary>
        /// 当前任务ID
        /// </summary>
        private string taskid { get; set; }
        public Loading ld { get; set; }
        /// <summary>
        /// 是否已加载完成
        /// </summary>
        private bool isLoadingCompleted { get; set; }

        /// <summary>
        /// 开始采集网址商品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddGoods_Click(object sender, EventArgs e)
        {
            string url = txtAddress.Text;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            if (isLoadingCompleted)
            {
                isLoadingCompleted = false;
                Runing = true;
                if (ld != null && !ld.IsDisposed)
                {
                    ld.Dispose();
                    ld.Close();
                }
                ld = new Loading();
                ld.StartPosition = FormStartPosition.Manual;
                RECT rect = new RECT();
                WinApi.GetWindowRect(this.Handle, ref rect);
                ld.Location = new Point(rect.Left, rect.Top);
                ld.Size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
                ld.Show(this);
            }
            new System.Threading.Thread(() =>
            {
                //建立采集计划
                taskid = LogicGoods.Instance.startDigOnePage(MyUserInfo.LoginToken, url);
                if (string.IsNullOrEmpty(taskid))
                {
                    Runing = false;
                    LoadingClose();
                }
                else
                {
                    while (Runing)
                    {
                        doPolling();
                        System.Threading.Thread.Sleep(5000);
                    }

                }
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 关闭loading
        /// </summary>
        private void LoadingClose()
        {
            if (!isLoadingCompleted)
            {
                if (this.ld != null && this.ld.InvokeRequired)
                {
                    this.ld.Invoke(new Action(LoadingClose), new object[] { });
                }
                else
                {
                    if (ld != null)
                        ld.Close();

                    isLoadingCompleted = true;
                }
            }
        }
        /// <summary>
        /// 轮询
        /// </summary>
        private void doPolling()
        {
            if (string.IsNullOrEmpty(taskid))
                return;
            var result = LogicGoods.Instance.queryGoods(MyUserInfo.LoginToken, taskid);
            if (result != null)
            {
                Runing = !result.finish;
                AlertTip("采集完成：共" + result.urls.Count() + "个");
                importGoods(result.urls);
            }
        }




        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="text"></param>
        public void AlertTip(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AlertTip), new object[] { text });
            }
            else
            {
                MessageAlert alert = new MessageAlert(text, "提示");
                alert.StartPosition = FormStartPosition.CenterScreen;
                alert.Show();
            }
        }


        /// <summary>
        /// 打开指定采集网
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenUrl(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            txtAddress.Text = btn.Tag.ToString();
            OpenGoodsUrl();
        }


        /// <summary>
        /// 商品导入
        /// </summary>
        /// <param name="data"></param>
        public void importGoods(List<GoodsCollertUrlsModel> data)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (var item in data)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["url"] = item.goodsUrl;
                dict["url2"] = item.couponUrl;
                dict["message"] = string.Format("{0}，券后【{1}元】包邮秒杀，领券地址：{2} \r\n 下单地址:{3} ", item.goodsName, item.couponAfterPrice.ToString("f2"), item.couponUrl, item.goodsName);
                list.Add(dict);
            }
            string jsonUrls = JsonConvert.SerializeObject(list);
            new System.Threading.Thread(() =>
            {
                //根据地址，获取商品优惠信息
                List<GoodsSelectedModel> goodsData = LogicGoods.Instance.getGoodsByLink(MyUserInfo.LoginToken, jsonUrls);
                try
                {
                    if (goodsData != null && goodsData.Count() > 0)
                    {
                        bool isUpdate = false;
                        foreach (var goods in goodsData)
                        {
                            try
                            {
                                //保存商品到本地数据库
                                LogicGoods.Instance.SaveGoods(goods, MyUserInfo.currentUserId, out isUpdate);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                LoadingClose();
                AlertTip("数据采集完成");
            })
            { IsBackground = true }.Start();
        }
    }
}
