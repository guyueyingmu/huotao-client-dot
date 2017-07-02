﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iQQ.Net.WebQQCore.Im.Bean;
using iQQ.Net.WebQQCore.Im.Event;
using iQQ.Net.WebQQCore.Util;

namespace QQLogin
{
    public partial class QQMainControl : UserControl
    {

        public QQMainControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 登录成功处理
        /// </summary>
        public event QQNotifyLoginSuccessEventHandler loginSuccessHandler;

        /// <summary>
        /// 关闭QQ监控
        /// </summary>
        public event CloseQQEventHandler CloseQQHandler;

        /// <summary>
        /// 生成商品
        /// </summary>
        public event BuildGoodsEventHandler BuildGoodsHandler;




        /// <summary>
        /// 是否显示自动跟发功能
        /// </summary>
        public bool IsShowAutoSend { get; set; } = true;

        /// <summary>
        /// 是否显示自定义文案功能
        /// </summary>
        public bool IsShowCustomTemplate { get; set; } = true;


        public QQLogin qqForm;

        private void QQMainControl_Load(object sender, EventArgs e)
        {
            if (!IsShowAutoSend)
            {
                ckbAutoSend.Visible = false;
                label1.Visible = false;
            }

            if (!IsShowCustomTemplate)
            {
                ckbEnableCustomTemplate.Visible = false;
                label4.Visible = false;
            }

            if (qqForm == null)
            {
                LoginQQ();
            }
        }


        /// <summary>
        /// 登录QQ
        /// </summary>
        private void LoginQQ()
        {
            if (qqForm == null)
            {
                qqForm = new QQLogin();
                qqForm.isShowQQGroupList = false;
                qqForm.CloseQQHandler += QqForm_CloseQQHandler;
                qqForm.loginSuccessHandler += QqForm_loginSuccessHandler;
                qqForm.GroupMsgHandler += QqForm_GroupMsgHandler;
                qqForm.GroupLoadSuccessHandler += QqForm_GroupLoadSuccessHandler;
                qqForm.ShowDialog(this);
            }
            else
            {
                CloseEx();
            }
        }


        /// <summary>
        /// qq群加载完成
        /// </summary>
        private void QqForm_GroupLoadSuccessHandler()
        {
            SetContactsView(QQGlobal.client.GetGroupList());
        }


        /// <summary>
        /// 群消息处理
        /// </summary>
        /// <param name="msgContent"></param>
        /// <param name="urls"></param>
        private void QqForm_GroupMsgHandler(long msgCode, long gid, string msgGroupName, string msgContent, List<string> urls)
        {

            QQGroup group = QQGlobal.listenGroups.Find(g => { return g.Gid == gid; });

            //TODO:接收群消息
            QQGroupMessageModel message = new QQGroupMessageModel()
            {
                GroupName = group != null ? group.GetGroupName() : msgGroupName,
                MessageContent = msgContent,
                MessageStatus = 0,
                Code = msgCode
            };
            if (urls != null)
            {
                if (urls.Count() > 0)
                    message.MessageUrl1 = urls[0];
                if (urls.Count() > 1)
                    message.MessageUrl2 = urls[1];
            }
            SetMessageView(message);
            if (BuildGoodsHandler != null)
                MessageHandler(urls, message);
        }


        /// <summary>
        /// 获取QQ号码
        /// </summary>
        /// <returns></returns>
        public string GetQQ()
        {
            return QQGlobal.account.QQ.ToString();
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="msgCode"></param>
        /// <param name="msgContent"></param>
        private void MessageHandler(List<string> urls, QQGroupMessageModel message)
        {
            //TODO:
            if (urls != null && urls.Count() > 0)
            {
                bool isAutoSend = ckbAutoSend.Checked;
                bool isEnableCustom = ckbEnableCustomTemplate.Checked;
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        //消息处理回调
                        BuildGoodsHandler?.Invoke(message.Code, message.GroupName, message.MessageContent, urls, isAutoSend, isEnableCustom, (ret, i, t) =>
                          {
                              string text = "";
                              switch (ret)
                              {
                                  case MessageCallBackType.正在准备:
                                      text = "正在准备";
                                      break;
                                  case MessageCallBackType.开始转链:
                                      text = string.Format("开始转链{0}/{1}", i, t);
                                      break;
                                  case MessageCallBackType.转链完成:
                                      text = string.Format("转链完成");
                                      break;
                                  case MessageCallBackType.开始创建计划:
                                      text = string.Format("创建计划..");
                                      break;
                                  case MessageCallBackType.完成:
                                      text = string.Format("已完成");
                                      break;
                                  case MessageCallBackType.未准备:
                                      text = "未准备";
                                      break;
                                  case MessageCallBackType.失败:
                                      text = "失败";
                                      break;
                                  default:
                                      break;
                              }
                              if (!string.IsNullOrEmpty(text))
                                  SetMesageViewByMessageCode(message.Code, text);
                          });

                    }
                    catch (Exception)
                    {
                        SetMesageViewByMessageCode(message.Code, "已完成");
                    }
                })
                { IsBackground = true }.Start();

            }
        }

        /// <summary>
        /// QQ登录成功回调事件
        /// </summary>
        private void QqForm_loginSuccessHandler()
        {
            QQGlobal.client.GetUserFace(QQGlobal.account, (s, e) =>
            {
                if (e.Type == QQActionEventType.EvtOK)
                {
                    SetQQLogo(QQGlobal.account.Face);
                    SetLabelTitle(lbQQAccount, QQGlobal.account.QQ.ToString());
                    SetLabelTitle(lbQQNickName, QQGlobal.account.Nickname);
                }
            });

            LoadingShow();
            //TODO:处理登录成功后的逻辑
            loginSuccessHandler?.Invoke();
        }

        /// <summary>
        /// 点击关闭按钮回调事件
        /// </summary>
        private void QqForm_CloseQQHandler()
        {
            //TODO:关闭窗口
            CloseQQHandler?.Invoke();
        }


        /// <summary>
        /// 关闭窗体
        /// </summary>
        public void CloseEx()
        {
            qqForm.Close();
            qqForm = null;
            QQGlobal.Reset();
        }

        private void SetLabelTitle(Label lbControl, string text)
        {
            if (lbControl.InvokeRequired)
            {
                lbControl.Invoke(new Action<Label, string>(SetLabelTitle), new object[] { lbControl, text });
            }
            else
            {
                lbControl.Text = text;
            }
        }

        private void SetQQLogo(Image logo)
        {
            if (picLogo.InvokeRequired)
            {
                picLogo.Invoke(new Action<Image>(SetQQLogo), new object[] { logo });
            }
            else
            {
                picLogo.Image = logo;
            }
        }



        /// <summary>
        /// 显示loading
        /// </summary>
        private void LoadingShow()
        {
            if (this.picLoading.InvokeRequired)
            {
                this.picLoading.Invoke(new Action(LoadingShow));
            }
            else
            {
                picLoading.Visible = true;
            }
        }

        /// <summary>
        /// 加载微信通讯录
        /// </summary>
        /// <param name="contact_all">The contact_all.</param>
        public void SetContactsView(List<QQGroup> contact_all)
        {
            if (dgvContact.InvokeRequired)
            {
                this.dgvContact.Invoke(new Action<List<QQGroup>>(SetContactsView), new object[] { contact_all });
            }
            else
            {
                this.dgvContact.Rows.Clear();
                int i = dgvContact.Rows.Count;
                foreach (var user in contact_all)
                {
                    dgvContact.Rows.Add();
                    ++i;

                    dgvContact.Rows[i - 1].Cells["GroupGid"].Value = user.Gid;
                    var group = QQGlobal.listenGroups != null ? QQGlobal.listenGroups.Find((g) => { return g.Gid == user.Gid; }) : null;

                    if (group != null)
                        dgvContact.Rows[i - 1].Cells["GroupTitle"].Value = group.GetGroupName() + (group.isListen ? "(已监控)" : "");
                    else
                        dgvContact.Rows[i - 1].Cells["GroupTitle"].Value = user.GetGroupName();

                    dgvContact.Rows[i - 1].DefaultCellStyle.BackColor = QQGlobal.backColor;
                    dgvContact.Rows[i - 1].DefaultCellStyle.SelectionBackColor = QQGlobal.backColor;
                }
                picLoading.Visible = false;
                btnLogoutQQ.Visible = true;
                new System.Threading.Thread(() =>
                {
                    while (string.IsNullOrEmpty(QQGlobal.account.Nickname)) { }
                    SetLabelTitle(lbQQNickName, QQGlobal.account.Nickname);
                })
                { IsBackground = true }.Start();
            }
        }

        /// <summary>
        /// 加载微信通讯录
        /// </summary>
        /// <param name="contact_all">The contact_all.</param>
        public void SetMessageView(QQGroupMessageModel message)
        {
            if (dgvMessageView.InvokeRequired)
            {
                this.dgvMessageView.Invoke(new Action<QQGroupMessageModel>(SetMessageView), new object[] { message });
            }
            else
            {
                int i = dgvMessageView.Rows.Count;
                dgvMessageView.Rows.Add();
                ++i;
                dgvMessageView.Rows[i - 1].Cells["MessageCode"].Value = message.Code;
                dgvMessageView.Rows[i - 1].Cells["GroupName"].Value = message.GroupName;
                dgvMessageView.Rows[i - 1].Cells["MessageContent"].Value = message.MessageContent;
                dgvMessageView.Rows[i - 1].Cells["MessageUrl1"].Value = message.MessageUrl1;
                dgvMessageView.Rows[i - 1].Cells["MessageUrl2"].Value = message.MessageUrl2;
                dgvMessageView.Rows[i - 1].Cells["MessageStatus"].Value = message.MessageStatus == 0 ? "未处理" : "已完成";
                dgvMessageView.Rows[i - 1].Cells["Status"].Value = 0;

                if (i % 2 == 0)
                {
                    dgvMessageView.Rows[i - 1].DefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
                    dgvMessageView.Rows[i - 1].DefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 248, 248);
                }
                else
                {
                    dgvMessageView.Rows[i - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    dgvMessageView.Rows[i - 1].DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 255, 255);
                }
                dgvMessageView.Rows[i - 1].DefaultCellStyle.ForeColor = Color.FromArgb(180, 180, 180);
                dgvMessageView.CurrentCell = dgvMessageView.Rows[dgvMessageView.Rows.Count - 1].Cells[dgvMessageView.CurrentCell.ColumnIndex];
            }
        }

        /// <summary>
        /// 根据消息Codo，修改状态信息
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="msgText"></param>
        public void SetMesageViewByMessageCode(long Code, string msgText)
        {
            if (dgvMessageView.InvokeRequired)
            {
                this.dgvMessageView.Invoke(new Action<long, string>(SetMesageViewByMessageCode), new object[] { Code, msgText });
            }
            else
            {
                foreach (DataGridViewRow row in dgvMessageView.Rows)
                {
                    if (row.Cells["MessageCode"].Value.ToString().Equals(Code.ToString()))
                    {
                        row.Cells["MessageStatus"].Value = msgText;
                        break;
                    }
                }
            }
        }



        /// <summary>
        /// 鼠标进入单元格时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvContact_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            currentRowIndex = e.RowIndex;
            dgvContact.Rows[e.RowIndex].DefaultCellStyle.BackColor = QQGlobal.backColorSelected;
            dgvContact.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = QQGlobal.backColorSelected;
        }

        /// <summary>
        /// 鼠标离开单元格时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvContact_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = this.dgvContact.CurrentRow.Cells;
            if (cells[0].RowIndex != e.RowIndex)
            {
                dgvContact.Rows[e.RowIndex].DefaultCellStyle.BackColor = QQGlobal.backColor;
                dgvContact.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = QQGlobal.backColor;
            }
        }
        /// <summary>
        /// 点击单元格时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvContact_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = this.dgvContact.CurrentRow.Cells;
            foreach (DataGridViewRow row in this.dgvContact.Rows)
            {
                if (row.Cells[0].RowIndex != e.RowIndex)
                {
                    row.DefaultCellStyle.BackColor = QQGlobal.backColor;
                    row.DefaultCellStyle.SelectionBackColor = QQGlobal.backColor;
                }
            }
        }



        /// <summary>
        /// 当前鼠标所在的行索引
        /// </summary>
        private int currentRowIndex { get; set; }
        private void cmsTools_Opening(object sender, CancelEventArgs e)
        {
            DataGridViewCellCollection cells = this.dgvContact.CurrentRow.Cells;
            if (cells == null) e.Cancel = true;
            if (cells[0].RowIndex != currentRowIndex)
                e.Cancel = true;
            else
            {
                long gid = (long)cells["GroupGid"].Value;
                if (QQGlobal.listenGroups.Exists((g) => { return g.Gid == gid && g.isListen; }))
                    toolAddListen.Text = "移除监控";
                else
                    toolAddListen.Text = "添加监控";
            }
        }
        /// <summary>
        /// 添加监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolAddListen_Click(object sender, EventArgs e)
        {
            if (this.dgvContact.CurrentRow == null) return;

            DataGridViewCellCollection cells = this.dgvContact.CurrentRow.Cells;
            if (cells == null) return;
            long gid = (long)cells["GroupGid"].Value;
            QQGroup group = QQGlobal.store.GetGroupByGin(gid);

            if (QQGlobal.listenGroups.Exists((g) => { return g.Gid == gid; }))
            {
                QQGlobal.listenGroups.ForEach(item =>
                {
                    if (item.Gid == gid)
                        item.isListen = !item.isListen;
                });

                QQGroup gg = QQGlobal.listenGroups.Find(g => { return g.Gid == gid; });
                cells["GroupTitle"].Value = group.GetGroupName() + (gg.isListen ? "(已监控)" : "");
            }
            else
            {
                group.isListen = true;
                QQGlobal.listenGroups.Add(group);
                cells["GroupTitle"].Value = group.GetGroupName() + "(已监控)";
            }
        }

        /// <summary>
        /// 修改别名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolsUpdateAlias_Click(object sender, EventArgs e)
        {
            if (this.dgvContact.CurrentRow == null) return;
            DataGridViewCellCollection cells = this.dgvContact.CurrentRow.Cells;
            if (cells == null) return;
            long gid = (long)cells["GroupGid"].Value;
            QQGroup group = QQGlobal.listenGroups.Find(g => { return g.Gid == gid; });
            if (group != null)
                cells["GroupTitle"].Value = group.GetGroupName();

            cells["GroupTitle"].ReadOnly = false;
            //cells["GroupTitle"].
            dgvContact.BeginEdit(true);
        }



        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogoutQQ_Click(object sender, EventArgs e)
        {
            CloseEx();
            btnLogoutQQ.Visible = false;
            picLogo.Image = Properties.Resources.QQ40x40;
            LoginQQ();
        }

        private void dgvMessageView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = this.dgvMessageView.CurrentRow.Cells;
            if (cells != null && cells["DeteleMessage"].ColumnIndex == e.ColumnIndex)
            {
                this.dgvMessageView.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void cmsToolsResult_Opening(object sender, CancelEventArgs e)
        {
            if (this.dgvMessageView.Rows == null || this.dgvMessageView.Rows.Count == 0 || this.dgvMessageView.CurrentRow == null)
            {
                e.Cancel = true;
                return;
            }
            DataGridViewCellCollection cells = this.dgvMessageView.CurrentRow.Cells;
            if (cells == null) e.Cancel = true;
            if (cells[0].RowIndex != currentRowIndex)
                e.Cancel = true;
            else
            {
                string MessageStatus = cells["MessageStatus"].Value.ToString();
                if (!MessageStatus.Equals("未处理")|| !MessageStatus.Equals("失败"))
                    e.Cancel = true;
            }
        }

        private void dgvMessageView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            currentRowIndex = e.RowIndex;
        }
        /// <summary>
        /// 重新跟发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolReSend_Click(object sender, EventArgs e)
        {
            if (this.dgvMessageView.Rows == null) return;
            DataGridViewCellCollection cells = this.dgvMessageView.Rows[currentRowIndex].Cells;
            if (cells != null)
            {
                string GroupName = cells["GroupName"].Value.ToString();
                string msgContent = cells["MessageContent"].Value.ToString();
                long msgCode = (long)cells["MessageCode"].Value;
                var urls = UrlUtils.GetUrls(msgContent);
                if (BuildGoodsHandler != null)
                {
                    QQGroupMessageModel message = new QQGroupMessageModel()
                    {
                        MessageContent = msgContent,
                        Code = msgCode,
                        GroupName = GroupName
                    };
                    MessageHandler(urls, message);
                }
            }

        }
        /// <summary>
        /// 结束单元格编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvContact_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                dgvContact[e.ColumnIndex, e.RowIndex].ReadOnly = true;
                dgvContact.EndEdit();
                DataGridViewCellCollection cells = dgvContact.Rows[e.RowIndex].Cells;
                if (cells == null) return;

                long gid = (long)cells["GroupGid"].Value;
                string aliasName = cells["GroupTitle"].Value.ToString();

                QQGroup group = QQGlobal.store.GetGroupByGin(gid);

                var it = QQGlobal.listenGroups.Find((g) => { return g.Gid == gid; });
                if (it != null)
                {
                    QQGlobal.listenGroups.ForEach(item =>
                    {
                        if (item.Gid == gid)
                            item.Alias = aliasName;
                    });

                    cells["GroupTitle"].Value = aliasName + (it.isListen ? "(已监控)" : "");
                }
                else
                {
                    group.Alias = aliasName;
                    QQGlobal.listenGroups.Add(group);
                }
            }
            catch (Exception)
            {

            }

        }

        private void ckbEnableCustomTemplate_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Checked)
                ckbAutoSend.Checked = cb.Checked;
            ckbAutoSend.Enabled = !cb.Checked;
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolClearAll_Click(object sender, EventArgs e)
        {
            dgvMessageView.Rows.Clear();
        }
    }
}
