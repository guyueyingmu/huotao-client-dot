﻿/*
 * 版权所有:杭州火图科技有限公司
 * 地址:浙江省杭州市滨江区西兴街道阡陌路智慧E谷B幢4楼在地图中查看
 * (c) Copyright Hangzhou Hot Technology Co., Ltd.
 * Floor 4,Block B,Wisdom E Valley,Qianmo Road,Binjiang District
 * 2013-2017. All rights reserved.
 * author guomw
**/


using HotJoinImage;
using HOTReuestService;
using HOTReuestService.Helper;
using HotTaoCore.DAL;
using HotTaoCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static HotTaoCore.Models.SQLiteEntitysModel;

namespace HotTaoCore.Logic
{
    public class LogicHotTao
    {
       static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static LogicHotTao _instance = null;
        private static HotTaoDAL dal;

        private static HotTaoDAL lnDal = new HotTaoDAL(0);

        private static int uid { get; set; }

        private LogicHotTao()
        {

        }

        public static LogicHotTao Instance(int userid)
        {
            uid = userid;
            if (_instance == null)
                _instance = new LogicHotTao();
            if (dal == null && userid > 0)
                dal = new HotTaoDAL(userid);
            return _instance;
        }


        public void CloseConnection()
        {
            if (dal != null)
                dal.CloseConnection();
        }



        #region 微信群操作
        /// <summary>
        /// 添加用户微信群
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public int AddUserWeChatGroup(weChatGroupModel model)
        {
            return dal.AddUserWeChatGroup(model);
        }

        /// <summary>
        /// 修改微信群
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserWeChatGroup(weChatGroupModel model)
        {
            return dal.UpdateUserWeChatGroup(model);
        }
        /// <summary>
        /// 修改微信pid
        /// </summary>
        /// <param name="groupid">The groupid.</param>
        /// <param name="pid">The pid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserWeChatGroup(int groupid, string pid)
        {
            return dal.UpdateUserWeChatGroup(groupid, pid);
        }

        public bool UpdateUserWeChatTitle(int groupid, string title, string pid, int groupType)
        {
            weChatGroupModel model = new weChatGroupModel()
            {
                id = groupid,
                pid = pid,
                title = title,
                userid = uid,
                type = groupType
            };
            if (groupid > 0)
                return UpdateUserWeChatGroup(model);
            else
                return AddUserWeChatGroup(model) > 0;
        }

        /// <summary>
        /// 删除微信群
        /// </summary>
        /// <param name="groupid">The groupid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteUserWeChatGroup(List<int> groupid)
        {
            return dal.DeleteUserWeChatGroup(groupid);
        }

        /// <summary>
        /// 根据微信群删除对应的分享内容
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteWeChatShareText(string title)
        {
            return dal.DeleteWeChatShareText(title);
        }
        /// <summary>
        /// 删除分享文本
        /// </summary>
        /// <param name="taskid">The taskid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteShareText(int taskid)
        {
            return dal.DeleteShareText(taskid);
        }
        /// <summary>
        /// 获取微信群信息
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>weChatGroupModel.</returns>
        public weChatGroupModel FindByUserWeChatGroup(string title, int userid)
        {
            return dal.FindByUserWeChatGroup(title, userid);
        }

        /// <summary>
        /// 根据用户id，获取微信群
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>List&lt;weChatGroupModel&gt;.</returns>
        public List<UserWechatListModel> GetUserWeChatGroupListByUserId(int userid)
        {
            List<UserWechatListModel> data = new List<UserWechatListModel>();
            var items = dal.GetUserWeChatGroupListByUserId(userid);
            if (items != null)
            {
                items.ForEach(item =>
                {

                    data.Add(new UserWechatListModel()
                    {
                        id = Convert.ToInt32(item.id),
                        pid = item.pid,
                        wechattitle = item.title,
                        userid = userid,
                        type = item.type
                    });
                });
            }
            return data;
        }

        /// <summary>
        /// 根据群id，获取群数据
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="ids">The ids.</param>
        /// <returns>weChatGroupModel.</returns>
        public List<weChatGroupModel> FindByUserWeChatGroup(int userid, List<int> ids)
        {
            return dal.FindByUserWeChatGroup(userid, ids);
        }



        #endregion





        #region 商品操作

        /// <summary>
        /// 添加本地商品
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public int AddUserGoods(GoodsModel model, out bool isUpdate)
        {
            return dal.AddUserGoods(model, out isUpdate);
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="gid">The gid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteGoods(int gid)
        {
            return dal.DeleteGoods(gid);
        }


        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public bool DeleteGoodsByGoodsid(string goodsId)
        {
            return dal.DeleteGoodsByGoodsid(goodsId);
        }

        /// <summary>
        /// 删除所有本地商品
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteAllGoods(int userid)
        {
            bool flag = dal.DeleteAllGoods(userid);
            if (flag)
                dal.DeleteAllTaskPlan(userid);
            return flag;
        }
        /// <summary>
        /// 删除选中的本地商品
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="ids">The ids.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteAllGoods(int userid, List<int> ids)
        {
            return dal.DeleteAllGoods(userid, ids);
        }

        /// <summary>
        /// 修改商品合成图片状态
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool UpdateGoodsJoinImageStatusByIds(int userid, List<int> ids)
        {
            return dal.UpdateGoodsJoinImageStatusByIds(userid, ids);
        }


        /// <summary>
        /// 根据商品自增ID，获取商品信息
        /// </summary>
        /// <param name="gid">The gid.</param>
        /// <returns>GoodsModel.</returns>
        public GoodsModel FindByUserGoodsInfo(int gid)
        {
            return dal.FindByUserGoodsInfo(gid);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>List&lt;GoodsModel&gt;.</returns>
        public List<GoodsModel> FindByUserGoodsList()
        {
            return dal.FindByUserGoodsList(uid);
        }


        /// <summary>
        /// 获取最早的三条未合成图片的商品数据
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>List&lt;GoodsModel&gt;.</returns>
        public List<GoodsModel> FindByUserGoodsListThree(int JoinImageCount)
        {
            return dal.FindByUserGoodsListThree(uid, JoinImageCount);
        }


        /// <summary>
        /// 根据商品id获取商品信息
        /// </summary>
        /// <param name="goodsId">The goods identifier.</param>
        /// <returns>GoodsModel.</returns>
        public GoodsModel FindByUserGoodsInfo(string goodsId, int userid)
        {
            return dal.FindByUserGoodsInfo(goodsId, userid);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="ids">The ids.</param>
        /// <returns>List&lt;GoodsModel&gt;.</returns>
        public List<GoodsModel> FindByUserGoodsList(List<int> ids)
        {
            return dal.FindByUserGoodsList(uid, ids);
        }


        #endregion



        #region 任务计划相关操作
        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <param name="gid">The gid.</param>
        /// <returns>TaskPlanModel.</returns>
        public TaskPlanModel FindByUserTaskPlanInfo(int taskid)
        {
            return dal.FindByUserTaskPlanInfo(uid, taskid);
        }

        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>List&lt;GoodsModel&gt;.</returns>
        public List<TaskPlanModel> FindUserTaskPlanListByUserId(bool isFilterFinish)
        {
            return dal.FindUserTaskPlanListByUserId(uid, isFilterFinish);
        }
        /// <summary>
        /// 删除微信群
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteUserTaskPlan(int taskid)
        {
            return dal.DeleteUserTaskPlan(taskid);
        }

        /// <summary>
        /// 删除所有计划列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool DeleteAllTaskPlan(int userid)
        {
            return dal.DeleteAllTaskPlan(userid);
        }


        /// <summary>
        /// 修改用户计划任务转链状态,转链成功后调用
        /// </summary>
        /// <param name="taskid">The taskid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserTaskPlanIsTpwd(int taskid)
        {
            return dal.UpdateUserTaskPlanIsTpwd(taskid);
        }

        /// <summary>
        /// 修改用户计划任务执行状态 0 待执行  1进行中， 2已完成 3已过期
        /// </summary>
        /// <param name="taskid">The taskid.</param>
        /// <param name="status">The status.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserTaskPlanExecStatus(int taskid, int status)
        {
            return dal.UpdateUserTaskPlanExecStatus(taskid, status);
        }
        /// <summary>
        /// 添加用户计划任务
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public TaskPlanModel AddUserTaskPlan(TaskPlanModel model)
        {
            if (model.id == 0)
            {
                int taskid = dal.AddUserTaskPlan(model);
                if (taskid > 0)
                {
                    return FindByUserTaskPlanInfo(taskid);
                }
            }
            else
            {
                if (dal.UpdateUserTaskPlan(model) > 0)
                    return FindByUserTaskPlanInfo(Convert.ToInt32(model.id));
            }
            return null;
        }

        /// <summary>
        /// 添加微信分享数据
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public int AddUserWechatSharetext(weChatShareTextModel model)
        {
            return dal.AddUserWechatSharetext(model);
        }


        /// <summary>
        /// 修改发送状态
        /// </summary>
        /// <param name="shareid">The shareid.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserShareTextStatus(long shareid)
        {
            return dal.UpdateUserShareTextStatus(shareid);
        }
        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="shareid">The shareid.</param>
        /// <param name="text">The text.</param>
        /// <param name="tpwd">The TPWD.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool UpdateUserShareTextStatus(long shareid, string text, string tpwd)
        {
            return dal.UpdateUserShareTextStatus(shareid, text, tpwd);
        }
        /// <summary>
        /// 添加微信发送失败数据
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public int AddUserWeChatError(weChatUserWechatErrorModel model)
        {
            return dal.AddUserWeChatError(model);
        }



        /// <summary>
        /// 获取发送内容列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="taskid">The taskid.</param>
        /// <returns>List&lt;GoodsModel&gt;.</returns>
        public List<weChatShareTextModel> FindByUserWechatShareTextList(int taskid)
        {
            return dal.FindByUserWechatShareTextList(uid, taskid);
        }
        /// <summary>
        /// 获取发送内容列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="taskid">The taskid.</param>
        /// <param name="goodsid">The goodsid.</param>
        /// <returns>List&lt;weChatShareTextModel&gt;.</returns>
        public List<weChatShareTextModel> FindByUserWechatShareTextList(int taskid, int goodsid)
        {
            return dal.FindByUserWechatShareTextList(uid, taskid, goodsid);
        }

        /// <summary>
        /// 获取发送内容列表
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <returns>List&lt;weChatShareTextModel&gt;.</returns>
        public List<weChatShareTextModel> FindByUserWechatShareTextList()
        {
            return dal.FindByUserWechatShareTextList(uid);
        }


        /// <summary>
        /// 生成淘口令
        /// </summary>
        /// <param name="loginToken">The login token.</param>
        /// <param name="userid">The userid.</param>
        /// <param name="taskid">The taskid.</param>
        /// <param name="templateText">文案</param>
        /// <param name="result">The result.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        public bool BuildTaskTpwd(string loginToken, int userid, int taskid, string templateText, string appkey, string appsecret, Action<weChatShareTextModel> result = null, bool append = false, bool isJoinImage = false)
        {
            var taskData = FindByUserTaskPlanInfo(taskid);
            if (taskData == null || taskData.ExecStatus != 0) return false;

            if (string.IsNullOrEmpty(taskData.pidsText) || string.IsNullOrEmpty(taskData.goodsText)) return false;

            List<UserPidTaskModel> lst = JsonConvert.DeserializeObject<List<UserPidTaskModel>>(taskData.pidsText);
            List<UserPidTaskModel> lst2 = JsonConvert.DeserializeObject<List<UserPidTaskModel>>(taskData.goodsText);
            List<int> ids = new List<int>();
            //如果群数据和商品数据都为空时
            if (lst == null || lst2 == null) return false;

            lst2.ForEach(item =>
            {
                if (!ids.Contains(item.id))
                    ids.Add(item.id);
            });
            //获取商品数据
            var goodslist = FindByUserGoodsList(ids);

            if (!append)
            {
                ids.Clear();
                lst.ForEach(item =>
                {
                    if (!ids.Contains(item.id))
                        ids.Add(item.id);
                });
                //获取微信群数据
                var wechatlist = FindByUserWeChatGroup(userid, ids);
                //删除现有数据
                dal.DeleteUserWechatShareText(userid, taskid);
                List<JoinGoodsList> joinLists = new List<JoinGoodsList>();
                foreach (var group in wechatlist)
                {
                    JoinGoodsList joinList = new JoinGoodsList();
                    //生成商品分享文本
                    BuildShareText(loginToken, userid, taskid, templateText, goodslist, group, appkey, appsecret, out joinList, result, isJoinImage);

                    if (isJoinImage && joinList != null)
                        joinLists.Add(joinList);
                }
                if (isJoinImage)
                {
                    int flag = BuildJoinImage(loginToken, joinLists, taskData.title, Convert.ToInt32(taskData.id), goodslist.Count());
                    if (flag > 0)
                        DeleteUserTaskPlan(flag);
                }
                UpdateUserTaskPlanIsTpwd(taskid);
            }
            else
            {
                var data = FindByUserWechatShareTextList(userid, taskid);
                if (data != null)
                {
                    //获取商品数据
                    var goods = goodslist[0];
                    data.ForEach(item =>
                    {
                        item.text += templateText;
                        item.text = BuildShareText(item.text, goods);
                        //修改内容
                        dal.UpdateUserShareText(item.id, item.text);

                        result?.Invoke(item);
                    });
                }
            }
            return true;
        }
        /// <summary>
        ///生成商品文案
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="taskid">The taskid.</param>
        /// <param name="templateText">The template text.</param>
        /// <param name="data">The data.</param>
        /// <param name="group">The group.</param>
        /// <returns>true if XXXX, false otherwise.</returns>
        private bool BuildShareText(string loginToken, int userid, int taskid, string templateText, List<GoodsModel> data, weChatGroupModel group, string appkey, string appsecret, out JoinGoodsList joinList, Action<weChatShareTextModel> result = null, bool isJoinImage = false)
        {
            joinList = null;
            if (data == null) return false;
            weChatShareTextModel share = new weChatShareTextModel()
            {
                userid = userid,
                taskid = taskid,
                status = -1,
                title = group.title
            };
            if (isJoinImage)
                joinList = new JoinGoodsList();

            List<int> ids = new List<int>();

            List<JoinGoodsInfo> imageList = new List<JoinGoodsInfo>();

            foreach (var item in data)
            {
                if (item.goodsPrice - item.couponPrice <= 0) continue;
                string url = GlobalConfig.couponUrl;
                url += "?src=ht_hot&activityId=" + item.couponId;
                url += "&itemId=" + item.goodsId.Replace("=", "");
                url += "&pid=" + (string.IsNullOrEmpty(group.pid) ? "mm_33648229_22032774_73500078" : group.pid);
                item.shareLink = url;
                string shortUrl = string.Empty;
                //将淘口令改成pid，2017-04-07 修改，淘口令改到分享时生产
                string tpwd = (string.IsNullOrEmpty(group.pid) ? "mm_33648229_22032774_73500078" : group.pid);// "[二合一淘口令]";// HotTaoApiService.Instance.taobao_wireless_share_tpwd_create(item.goodsMainImgUrl, item.shareLink, item.goodsName, appkey, appsecret);
                string text = templateText;

                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(tpwd))
                {
                    if (text.Contains("[商品标题]"))
                        text = text.Replace("[商品标题]", item.goodsName);
                    if (text.Contains("[商品价格]"))
                        text = text.Replace("[商品价格]", item.goodsPrice.ToString());
                    if (text.Contains("[券后价格]"))
                        text = text.Replace("[券后价格]", (item.goodsPrice - item.couponPrice).ToString());
                    if (text.Contains("[来源]"))
                        text = text.Replace("[来源]", item.goodsSupplier);
                    if (text.Contains("[销量]"))
                        text = text.Replace("[销量]", item.goodsSalesAmount.ToString());
                    if (text.Contains("[优惠券价格]"))
                        text = text.Replace("[优惠券价格]", item.couponPrice.ToString());
                    if (text.Contains("[分隔符]"))
                        text = text.Replace("[分隔符]", "-----------------");
                    if (text.Contains("[简介描述]"))
                        text = text.Replace("[简介描述]", string.IsNullOrEmpty(item.goodsIntro) ? "" : item.goodsIntro);

                    if (text.Contains("[商品地址]"))
                        text = text.Replace("[商品地址]", item.goodsDetailUrl);

                    if (text.Contains("[优惠券地址]"))
                        text = text.Replace("[优惠券地址]", item.couponUrl);

                }
                else
                    share.status = 2;
                share.goodsid = Convert.ToInt32(item.id);
                share.text = text;
                share.tpwd = tpwd;
                share.field1 = item.goodslocatImgPath;

                if (isJoinImage)
                {
                    string _url = GlobalConfig.couponUrl;
                    _url += "?src=ht_hot&activityId=" + item.couponId;
                    _url += "&itemId=" + item.goodsId.Replace("=", "");
                    _url += "&pid=" + tpwd;
                    var _tpwd = "";
                    share.field2 = _tpwd;

                    bool isLogin = true;

                    Tuple<string, string> resultTuple = TaobaoHelper.GetGaoYongToken(item.goodsDetailUrl, item.goodsId, tpwd, MyUserInfo.GetTbToken(), MyUserInfo.cookies, out isLogin);
                    string shareText = "";
                    if (resultTuple != null)
                    {
                        if (isLogin)
                            shareText = resultTuple.Item1;
                    }
                    //商品数据
                    joinList.collectionGoodsList.Add(new CollectionGoods()
                    {
                        goodsId = item.goodsId,
                        goodsName = item.goodsName,
                        price = item.goodsPrice,
                        discountAmount = item.couponPrice,
                        goodsPromotionUrl = _url,
                        goodsPrimaryImg = item.goodsMainImgUrl,
                        shareText = shareText
                    });
                    //图片
                    joinList.ImageList.Add(new JoinGoodsInfo()
                    {
                        GoodsName = item.goodsName,
                        GoodsPrice = item.goodsPrice,
                        CouponPrice = item.couponPrice,
                        imagePath = item.goodslocatImgPath,
                        GoodsIntro = item.goodsIntro
                    });

                }
                share.field6 = isJoinImage ? 1 : 0;
                share.field3 = group.id.ToString();
                AddUserWechatSharetext(share);
                result?.Invoke(share);
            }
            if (isJoinImage)
            {
                joinList.TaskId = taskid;
                joinList.id = Convert.ToInt32(group.id);
            }
            return true;
        }


        private string BuildShareText(string text, GoodsModel goods)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains("[商品标题]"))
                    text = text.Replace("[商品标题]", goods.goodsName);
                if (text.Contains("[商品价格]"))
                    text = text.Replace("[商品价格]", goods.goodsPrice.ToString());
                if (text.Contains("[券后价格]"))
                    text = text.Replace("[券后价格]", (goods.goodsPrice - goods.couponPrice).ToString());
                if (text.Contains("[来源]"))
                    text = text.Replace("[来源]", goods.goodsSupplier);
                if (text.Contains("[销量]"))
                    text = text.Replace("[销量]", goods.goodsSalesAmount.ToString());
                if (text.Contains("[优惠券价格]"))
                    text = text.Replace("[优惠券价格]", goods.couponPrice.ToString());
                if (text.Contains("[分隔符]"))
                    text = text.Replace("[分隔符]", "-----------------");
                if (text.Contains("[简介描述]"))
                    text = text.Replace("[简介描述]", string.IsNullOrEmpty(goods.goodsIntro) ? "" : goods.goodsIntro);
            }
            return text;
        }






        /// <summary>
        /// 生产淘口令
        /// </summary>
        /// <param name="currentUserId">The current user identifier.</param>
        /// <param name="LoginToken">The login token.</param>
        /// <param name="goods">The goods.</param>
        /// <param name="item">The item.</param>
        /// <param name="appkey">The appkey.</param>
        /// <param name="appsecret">The appsecret.</param>
        public bool BuildTpwd(int currentUserId, string LoginToken, GoodsModel goods, weChatShareTextModel item, string appkey, string appsecret)
        {
            string url = GlobalConfig.couponUrl;
            url += "?src=ht_hot&activityId=" + goods.couponId;
            url += "&itemId=" + goods.goodsId.Replace("=", "");
            url += "&pid=" + item.tpwd;
            string shortUrl = "";
            string tpwd = HotTaoApiService.Instance.taobao_wireless_share_tpwd_create(goods.goodsMainImgUrl, url, goods.goodsName, appkey, appsecret);
            if (!string.IsNullOrEmpty(tpwd))
            {
                if (item.text.Contains("[短链接]"))
                {
                    shortUrl = HotTaoApiService.Instance.buildShortUrl(LoginToken, tpwd, url, goods.goodsName, goods.goodsMainImgUrl);
                    if (string.IsNullOrEmpty(shortUrl))
                        shortUrl = HotTaoApiService.Instance.taobao_tbk_spread_get(url, appkey, appsecret);
                }
                if (item.text.Contains("[二合一淘口令]"))
                    item.text = item.text.Replace("[二合一淘口令]", tpwd);
                else
                    item.text += "复制这条信息，打开『手机淘宝』" + tpwd + "领券下单即可抢购宝贝";

                if (item.text.Contains("[短链接]"))
                    item.text = item.text.Replace("[短链接]", shortUrl);

                item.status = 0;
                UpdateUserShareTextStatus(item.id, item.text, tpwd);
                return true;
            }
            else
                return false;
        }
        #endregion

        #region 记住账号


        /// <summary>
        /// 添加记住用户
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>System.Int32.</returns>
        public int AddLoginName(LoginNameModel model)
        {
            return lnDal.AddLoginName(model);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="loginName">Name of the login.</param>
        /// <returns>LoginNameModel.</returns>
        public LoginNameModel GetLoginName(string loginName)
        {
            return lnDal.GetLoginName(loginName);
        }


        /// <summary>
        /// 获取账号列表
        /// </summary>
        /// <returns>List&lt;LoginNameModel&gt;.</returns>
        public List<LoginNameModel> GetLoginNameList()
        {
            return lnDal.GetLoginNameList();
        }
        /// <summary>
        /// 清空登录登录
        /// </summary>
        /// <returns></returns>
        public bool ClearLoginNameData()
        {
            return lnDal.ClearLoginNameData();
        }
        #endregion




        #region 获取当前执行的任务计划

        /// <summary>
        /// 获取即将或正在执行中的任务数据
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TaskPlanModel FindExecTaskPlanByUserId(int userId)
        {
            var data = FindUserTaskPlanListByUserId(true);
            if (data == null) return null;
            var lst = data.FindAll(item =>
             {
                 return (item.status == 0 || item.status == 1) && item.isTpwd != 0;
             });
            if (lst != null && lst.Count() > 0)
                return lst[0];
            return null;

        }
        #endregion




        /// <summary>
        /// 自动合成图片
        /// </summary>
        /// <param name="loginToken"></param>
        /// <param name="wechatlist"></param>
        /// <param name="appkey"></param>
        /// <param name="appsecret"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="JoinImageCount"></param>
        /// <returns></returns>
        public bool AutoJoinImage(string loginToken, List<UserWechatListModel> wechatlist, string appkey, string appsecret, DateTime startTime, DateTime endTime, int JoinImageCount)
        {

            string templateText = "[二合一淘口令]";
            List<UserPidTaskModel> pidList = new List<UserPidTaskModel>();
            List<UserPidTaskModel> goodsList = new List<UserPidTaskModel>();

            //判断群是否存在
            if (wechatlist == null || wechatlist.Count() <= 0)
                return false;
            else
            {
                foreach (var item in wechatlist)
                {
                    if (!pidList.Exists(r => r.id == item.id))
                        pidList.Add(new UserPidTaskModel() { id = item.id });
                }
            }
            //获取需要合成图片的商品数据
            var goodslist = FindByUserGoodsListThree(JoinImageCount);
            List<int> ids = new List<int>();
            if (goodslist != null && goodslist.Count() == JoinImageCount)
            {
                foreach (var item in goodslist)
                {
                    if (!goodsList.Exists(r => r.id == Convert.ToInt32(item.id)))
                        goodsList.Add(new UserPidTaskModel() { id = Convert.ToInt32(item.id) });


                    ids.Add(Convert.ToInt32(item.id));
                }
            }
            else
                return false;


            string goodsText = JsonConvert.SerializeObject(goodsList);
            string pidsText = JsonConvert.SerializeObject(pidList);
            TaskPlanModel model = new TaskPlanModel()
            {
                userid = uid,
                title = "【合成图片转发】",
                startTime = startTime,
                endTime = endTime,
                pidsText = pidsText,
                goodsText = goodsText,
                id = 0,
                status = 0,
                isTpwd = 0
            };
            TaskPlanModel data = AddUserTaskPlan(model);
            if (data == null) return false;
            List<JoinGoodsList> joinLists = new List<JoinGoodsList>();
            foreach (var group in wechatlist)
            {
                weChatGroupModel g = new weChatGroupModel()
                {
                    id = group.id,
                    title = group.wechattitle,
                    pid = group.pid,
                    userid = group.userid
                };
                JoinGoodsList joinList = new JoinGoodsList();
                //生成商品分享文本
                BuildShareText(loginToken, uid, Convert.ToInt32(data.id), templateText, goodslist, g, appkey, appsecret, out joinList, null, true);
                if (joinList != null)
                    joinLists.Add(joinList);
            }

            int flag = BuildJoinImage(loginToken, joinLists, data.title, Convert.ToInt32(data.id), JoinImageCount);
            if (flag == 0)
            {
                UpdateUserTaskPlanIsTpwd(Convert.ToInt32(data.id));
                UpdateGoodsJoinImageStatusByIds(uid, ids);
            }
            else
            {
                DeleteUserTaskPlan(flag);
            }

            return false;
        }
        /// <summary>
        /// 图片合成
        /// </summary>
        /// <param name="loginToken"></param>
        /// <param name="joinLists"></param>
        /// <param name="desc"></param>
        /// <param name="taskid"></param>
        /// <param name="JoinImageCount"></param>
        /// <returns></returns>
        private static int BuildJoinImage(string loginToken, List<JoinGoodsList> joinLists, string desc, int taskid, int JoinImageCount)
        {
            int result = 0;
            if (joinLists.Count() > 0)
            {
                #region 批量请求
                var goodsJson = JsonConvert.SerializeObject(joinLists);
                var collectionResult = LogicGoods.Instance.cacheCollectionGoods(loginToken, goodsJson);
                if (collectionResult != null)
                {
                    string path = System.Environment.CurrentDirectory + "\\temp\\joinimage";
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    foreach (var item in collectionResult)
                    {
                        try
                        {
                            var _data = joinLists.Find(r => r.id.ToString().Equals(item.Key));
                            if (_data == null) continue;
                            var img = JoinImage.GetGoodsJoinImage(_data.ImageList, string.Format("{0}?ids={1}", ApiDefineConst.QrCodeUrl, item.Value), string.IsNullOrEmpty(desc) ? "大牛精品选单" : desc.Replace("【合成图片转发】", ""));
                            if (img != null)
                            {
                                string fileName = EncryptHelper.MD5(_data.TaskId.ToString() + _data.id.ToString());
                                img.Save(string.Format("{0}\\{1}.jpg", path, fileName));
                                img.Dispose();
                            }
                            else
                            {
                                log.Debug("图片生成失败");
                            }
                        }
                        catch(Exception ex)
                        {
                            log.Error(ex);
                        }
                    }
                }
                else
                    result = joinLists[0].TaskId;
                #endregion
            }
            else
                result = taskid;

            return result;
        }
    }
}
