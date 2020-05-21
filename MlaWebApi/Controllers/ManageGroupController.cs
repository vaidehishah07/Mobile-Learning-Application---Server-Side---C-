using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MlaWebApi.Models;
////using System.Data.SqlServerCe;
using System.Data;
using System.Configuration;
using System.Windows.Forms;
using MlaWebApi;
//using System.Web.Script.Serialization;
using System.Web;
//using System.Web.SessionState.HttpSessionState;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Transactions;

namespace MlaWebApi.Controllers
{
    public class ManageGroupController : ApiController
    {
        public IQueryable GetTheFriendsToAddInGroup(int userId)
        {
            int Id_UserID = Convert.ToInt32(userId);
            using (var context_Var = new MlaDatabaseEntities1())
            {

                var userVar = (from friendT in context_Var.friendsTables
                               join adm in context_Var.admins
                               on friendT.friend_id equals adm.userId
                               join regUser in context_Var.registers
                               on adm.userId equals regUser.userId
                               where friendT.user_id == Id_UserID && friendT.Isaccepted == true
                               select new
                               {
                                   publickey = regUser.publicKey,
                                   firstName = adm.firstName,
                                   lastName = adm.lastName,
                                   userId = adm.userId,
                                   emailId = adm.emailId
                               }).ToList();
                return userVar.AsQueryable();
            }

        }
        public HttpResponseMessage CreateANewGroup(GroupClassCustom listOfGroup)
        {
            
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var context_Var = new MlaDatabaseEntities1())
                    {
                        int id1;

                
                        id1 = listOfGroup.OwnerId;
                        group grpobj = new group();
                        grpobj.userId = id1;
                        grpobj.groupType = 2;
                        grpobj.groupName = listOfGroup.groupName;
                        context_Var.groups.Add(grpobj);
                        context_Var.SaveChanges();

                        group_key grpkey = new group_key();
                        grpkey.groupNo = grpobj.groupNo;
                        grpkey.groupKey = listOfGroup.groupKey;
                        grpkey.userId = id1;
                        context_Var.group_key.Add(grpkey);
                        context_Var.SaveChanges();
                        res_Var = Request.CreateResponse<int>(System.Net.HttpStatusCode.Created, grpobj.groupNo);
                       



                    }
                    scope.Complete();
                    return res_Var;
                }
            }

            catch (Exception e)
            {
                res_Var = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return res_Var;
            }

        }


        public HttpResponseMessage CreateANewGroupWithFriends(GroupClassCustom l_grp)
        {
            var res_Var = Request.CreateResponse<int>(System.Net.HttpStatusCode.Created, 0);
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var context_Var = new MlaDatabaseEntities1())
                    {
                        int id1;

                        id_Int = l_grp.userId;
                        group_key grpkey = new group_key();
                        grpkey.groupNo = l_grp.groupId;
                        grpkey.groupKey = l_grp.groupKey;
                        grpkey.userId = l_grp;
                        context_Var.group_key.Add(grpkey);
                        context_Var.SaveChanges();
                        res_Var = Request.CreateResponse<int>(System.Net.HttpStatusCode.Created, l_grp.groupId);

                    }
                    scope.Complete();
                    return res_Var;
                }
            }

            catch (Exception e)
            {
                res_Var = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return res_Var;
            }

        }

        public IQueryable GetFriendListNotIngroup(int groupId, int userId)
        {

            using (var context_Var = new MlaDatabaseEntities1())
            {
                var lstfriend = (from grp in context_Var.group_key
                                 where grp.groupNo == groupId
                                 select grp.userId).ToList();

                var query = (from grp in context_Var.groups
                             join grpkey in context_Var.group_key
                             on grp.groupNo equals grpkey.groupNo
                             join reg in context_Var.registers
                             on grpkey.userId equals reg.userId
                             join admin in context_Var.admins
                             on reg.userId equals admin.userId
                             where
                             !lstfriend.Contains(grpkey.userId) &&
                             grp.userId == userId
                             && grp.groupType == 1
                             select new
                             {
                                 userId = reg.userId,
                                 publickey = reg.publicKey,
                                 firstName = admin.firstName,
                                 lastName = admin.lastName,
                                 emailId = admin.emailId
                             }).ToList();

                return query.AsQueryable();
            }


        }

        public IQueryable GetGroupListByUser(int user_Id_1)
        {
            using (var context_Var = new MlaDatabaseEntities1())
            {
                var query = (from grp in context_Var.groups
                             join grpkey in context_Var.group_key
                             on grp.groupNo equals grpkey.groupNo
                             where grpkey.userId == user_Id_1
                             && grp.groupType == 2
                             && grp.userId == user_Id_1
                             
                             select new
                             {
                                 userId = grp.userId,
                                 groupname = grp.groupName,
   
                                 groupKey = grpkey.groupKey,
                                 groupId = grp.groupNo,
                                 groupType = grp.groupType
                             }).ToList();

                return query.AsQueryable();
            }


        }


        public HttpResponseMessage GroupDel(int user_id_1)
        {
            try
            {

                using (var context_Var = new MlaDatabaseEntities1())
                {

                    var postlst = context_Var.posts.Where(x => x.groupNo == user_id_1).ToList();
                    foreach (var item in postlst)
                    {
                        context_Var.posts.Remove(item);
                    }

                    var grpkey = context_Var.group_key.Where(x => x.groupNo == user_id_1).ToList();
                    foreach (var item in grpkey)
                    {
                        context_Var.group_key.Remove(item);
                    }
                    var frnds = context_Var.friendsTables.Where(x => x.groupNo == user_id_1).ToList();

                    foreach (var item in frnds)
                    {
                        context_Var.friendsTables.Remove(item);
                    }
                    var grp = context_Var.groups.Where(x => x.groupNo == user_id_1).ToList();
                    foreach (var item in grp)
                    {
                        context_Var.groups.Remove(item);
                    }
                    context_Var.SaveChanges();
                }
                var res_Var = Request.CreateResponse<string>(System.Net.HttpStatusCode.Created, "Success");

                return res_Var;

            }
            catch (Exception e)
            {
                var res_Var = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return res_Var;
            }

        }





    }
}
