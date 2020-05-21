using MlaWebApi.Models;
using System;
using System.Collections.Generic;
////using System.Data.SqlServerCe;
using System.Data;
using System.Linq;
using System.Net.Http;
//using System.Web.Script.Serialization;
//using System.Web.SessionState.HttpSessionState;
using System.Transactions;
using System.Web.Http;

namespace MlaWebApi.Controllers
{
    public class ManageFriendsController : ApiController
    {
        //for loading a list of users for friend request
        public IQueryable GetFriendforReq(int currentUserId)
        {
            int Id = Convert.ToInt32(currentUserId);
            using (var ctx = new MlaDatabaseEntities1())
            {
                var getFriendVar = (from regUsers in ctx.registers
                                    join grp in ctx.groups on regUsers.userId equals grp.userId
                                    join grpkey in ctx.group_key on grp.groupNo equals grpkey.groupNo
                                    where grp.userId == Id && grp.groupType == 1
                                    select new { groupkey = grpkey.groupKey }).ToList();
                string groupKeyStr = getFriendVar[0].groupkey;
                var friendVar = (from std in ctx.friendsTables where std.user_id == Id select std.friend_id)
                                .Union(from std in ctx.friendsTables where std.friend_id == Id select std.user_id).ToList();
                var userVar = (from adm in ctx.admins
                               join reg in ctx.registers
                               on adm.userId equals reg.userId
                               where !friendVar.Contains(adm.userId)
                               && adm.userId != Id
                               select new
                               {
                                   publicKey = reg.publicKey,
                                   groupKey = groupKeyStr,
                                   userfirstName = adm.firstName,
                                   userlastName = adm.lastName,
                                   userId = adm.userId,
                                   email = adm.emailId,
                                   flag = ""
                               }).ToList();
                var fVar = (from ft1 in ctx.friendsTables
                            join reg in ctx.registers
                            on ft1.user_id equals reg.userId
                            join admin in ctx.admins
                            on reg.userId equals admin.userId
                            where ft1.friend_id == Id && ft1.Isaccepted == false

                            select new
                            {
                                publicKey = reg.publicKey,
                                groupKey = groupKeyStr,
                                userfirstName = admin.firstName,
                                userlastName = admin.lastName,
                                userId = admin.userId,
                                email = admin.emailId,
                                flag = "Approve friend Request"
                            }).ToList();
                userVar = userVar.Union(fVar).OrderBy(x => x.flag).ToList();

                return userVar.AsQueryable();
            }

        }



        public HttpResponseMessage AddAsFriend(EncryptionForFriends listofFriends)
        {
            try
            {

                int flagint = -1;
                flagint = listofFriends.friendId;

                using (TransactionScope scope = new TransactionScope())
                {
                    using (var ctx = new MlaDatabaseEntities1())
                    {

                        int fIdint = Convert.ToInt32(listofFriends.friendId);
                        var groupIdfromVar = ctx.groups.Where(x => x.userId == listofFriends.userId && x.groupType == 1).Select(x => x.groupNo).ToList();
                        int grpfromint = groupIdfromVar[0];
                        friendsTable fobj = new friendsTable();
                        fobj.user_id = listofFriends.userId;
                        fobj.friend_id = fIdint;
                        fobj.Isaccepted = false;
                        fobj.groupNo = grpfromint;
                        ctx.friendsTables.Add(fobj);

                        group_key grk1 = new group_key();
                        grk1.groupNo = grpfromint;
                        grk1.groupKey = listofFriends.groupKey;
                        grk1.userId = fIdint;
                        ctx.group_key.Add(grk1);

                        ctx.SaveChanges();
                    }
                    scope.Complete();
                }

                var response = Request.CreateResponse<int>(System.Net.HttpStatusCode.Created, flagint);
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return response;
            }

        }

        public IQueryable GetRequestedFriendList(int userId)
        {
            int fIdint = Convert.ToInt32(userId);
            using (var ctx = new MlaDatabaseEntities1())
            {
                var regVar = (from users in ctx.registers
                              join grp in ctx.groups
                              on users.userId equals grp.userId
                              join grpkey in ctx.group_key
                              on grp.groupNo equals grpkey.groupNo
                              where grp.userId == fIdint && grp.groupType == 1
                              select new { groupkey = grpkey.groupKey, groupId = grp.groupNo }).ToList();
                string groupkey = regVar[0].groupkey;
                int groupId = regVar[0].groupId;
                var frdVar = (from std in ctx.friendsTables
                              where std.friend_id == fIdint && std.Isaccepted == false
                              select new
                              {
                                  groupid = std.groupNo,
                                  fromuserId = std.user_id
                              }).ToList();
                var usersVar = (from flist in frdVar
                                join adm in ctx.admins
                                on flist.fromuserId equals adm.userId
                                join reg in ctx.registers
                                on adm.userId equals reg.userId
                                select new
                                {
                                    publicKey = reg.publicKey,
                                    groupNo = groupId,
                                    groupKey = groupkey,
                                    userfirstName = adm.firstName,
                                    userlastName = adm.lastName,
                                    userId = adm.userId,
                                    email = adm.emailId
                                }).ToList();
                return usersVar.AsQueryable();
            }

        }

        public HttpResponseMessage AcceptFriendRequest(int userId, int friendId, string groupkey, string groupId)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var ctx = new MlaDatabaseEntities1())
                    {
                        int groupIDint = Convert.ToInt32(groupId);
                        friendsTable fobj = new friendsTable();
                        fobj.user_id = userId;
                        fobj.friend_id = friendId;
                        fobj.Isaccepted = false;
                        fobj.groupNo = groupIDint;
                        ctx.friendsTables.Add(fobj);
                        ctx.SaveChanges();
                        var lstfrndVar = (from ft in ctx.friendsTables
                                          where ft.user_id == friendId && ft.friend_id == userId
                                          select ft).Union(from std in ctx.friendsTables
                                                           where std.friend_id == friendId && std.user_id == userId
                                                           select std).ToList();
                        lstfrndVar.ForEach(c => c.Isaccepted = true);
                        group_key gkobj = new group_key();
                        gkobj.userId = friendId;
                        gkobj.groupNo = Convert.ToInt32(groupId);
                        gkobj.groupKey = groupkey;
                        var query = ctx.group_key.Add(gkobj);
                        ctx.SaveChanges();
                    }

                    scope.Complete();
                }
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Created, "Success");
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return response;
            }

        }

        /* public HttpResponseMessage RejectFriendrequest(int userId, int fromuserId)
         {
             try
             {

                 using (TransactionScope scope = new TransactionScope())
                 {
                     using (var ctx = new MlaDatabaseEntities1())
                     {
                         var lstfrndVar = (from ft1 in ctx.friendsTables
                                        where ft1.friend_id == fromuserId && ft1.user_id == userId
                                        select ft1).ToList();
                         int? groupIdint = lstfrndVar[0].groupNo;
                         foreach (var valueOfFriend in lstfrndVar)
                         {
                             ctx.friendsTables.Remove(valueOfFriend);
                         }
                         var grpkeyVar = (from gk in ctx.group_key
                                       where gk.groupNo == groupIdint && gk.userId == userId
                                       select gk).ToList();
                         foreach (var valueOfFriend in grpkeyVar)
                         {
                             ctx.group_key.Remove(valueOfFriend);
                         }

                         ctx.SaveChanges();
                     }
                     scope.Complete();
                 }

                 var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Created, "Success");
                 return response;
             }
             catch (Exception e)
             {
                 var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                 return response;
             }

         }*/


        public HttpResponseMessage RejectFriendrequest(int userId, int fromuserId)
        {
            try
            {

                using (var ctx = new MlaDatabaseEntities1())
                {

                    var lstfrnd = (from std in ctx.friendsTables
                                   where std.friend_id == fromuserId && std.user_id == userId
                                   select std).ToList();
                    int? groupId = lstfrnd[0].groupNo;
                    foreach (var item in lstfrnd)
                    {
                        ctx.friendsTables.Remove(item);
                    }
                    var grpkey = (from gk in ctx.group_key
                                  where gk.groupNo == groupId && gk.userId == userId
                                  select gk).ToList();
                    foreach (var item in grpkey)
                    {
                        ctx.group_key.Remove(item);
                    }

                    ctx.SaveChanges();

                }
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Created, "Success");
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "Error");
                return response;
            }


        }
    }
}
