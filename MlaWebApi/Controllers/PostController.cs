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

using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Transactions;

namespace MlaWebApi.Controllers
{
    public class PostController : ApiController
    {
        
        public IQueryable GetAllPost(int userId)
        {

            using (var ctx = new MlaDatabaseEntities1())
            {
                var query = (from post in ctx.posts
                             join grpkey in ctx.group_key
                             on post.groupNo equals grpkey.groupNo
                             join reg in ctx.registers
                             on post.ownerId equals reg.userId
                             join admin in ctx.admins
                             on reg.userId equals admin.userId
                             join orgpost in ctx.posts
                             on post.originalPostId equals orgpost.postId into gj
                             from subpost in gj.DefaultIfEmpty()
                             join orgadmin in ctx.admins
                             on subpost.ownerId equals orgadmin.userId into gj1
                             from subadmin in gj1.DefaultIfEmpty()
                             where post.ownerId == userId
                             && grpkey.userId == userId
                             select new
                             {
                                 firstname = admin.firstName,
                                 lastname = admin.lastName,
                                 publickey = reg.publicKey,
                                 postId = post.postId,
                                 postType = post.postType,
                                 postText = post.postText,
                                 sessionKey = post.sessionKey,
                                 groupId = post.groupNo,
                                 ownerId = post.ownerId,
                                 originalPostId = post.originalPostId,
                                 timeStamp = post.timeStamp,
                                 digitalSignature = post.digitalSignature,
                                 groupkey = grpkey.groupKey,
                                 originalownerId = subpost.ownerId,
                                 originalfirstname = subadmin.firstName,
                                 originallastname = subadmin.lastName

                             }
                             ).Union
                            (from grpkey in ctx.group_key
                             join grp in ctx.groups
                             on grpkey.groupNo equals grp.groupNo
                             join post in ctx.posts
                             on grp.groupNo equals post.groupNo
                             join reg in ctx.registers
                             on post.ownerId equals reg.userId
                             join admin in ctx.admins
                             on reg.userId equals admin.userId
                             join orgpost in ctx.posts
                             on post.originalPostId equals orgpost.postId into gj
                             from subpost in gj.DefaultIfEmpty()
                             join orgadmin in ctx.admins
                           on subpost.ownerId equals orgadmin.userId into gj1
                             from subadmin in gj1.DefaultIfEmpty()
                             where grpkey.userId == userId
                             select new
                             {
                                 firstname = admin.firstName,
                                 lastname = admin.lastName,
                                 publickey = reg.publicKey,
                                 postId = post.postId,
                                 postType = post.postType,
                                 postText = post.postText,
                                 sessionKey = post.sessionKey,
                                 groupId = post.groupNo,
                                 ownerId = post.ownerId,
                                 originalPostId = post.originalPostId,
                                 timeStamp = post.timeStamp,
                                 digitalSignature = post.digitalSignature,
                                 groupkey = grpkey.groupKey,
                                 originalownerId = subpost.ownerId,
                                 originalfirstname = subadmin.firstName,
                                 originallastname = subadmin.lastName
                             }).Union(
                    from post in ctx.posts
                    join admin in ctx.admins
                    on post.ownerId equals admin.userId
                    where post.postType == 4
                    select new
                    {
                        firstname = admin.firstName,
                        lastname = admin.lastName,
                        publickey = "",
                        postId = post.postId,
                        postType = post.postType,
                        postText = post.postText,
                        sessionKey = post.sessionKey,
                        groupId = post.groupNo,
                        ownerId = post.ownerId,
                        originalPostId = post.originalPostId,
                        timeStamp = post.timeStamp,
                        digitalSignature = post.digitalSignature,
                        groupkey = "",
                        originalownerId = (long?)0,
                        originalfirstname = "",
                        originallastname = ""
                    }).ToList();

                return query.AsQueryable();
            }

        }
        public HttpResponseMessage AddPost(post post)
        {
           

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var ctx = new MlaDatabaseEntities1())
                    {
                        var query = ctx.posts.Add(post);
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

        public IQueryable GetGroupListforPost(int groupUserId, int grouptype, int? groupId)
        {
                if(groupId==0)
            {
                groupId = 0;
            }
            using (var ctx = new MlaDatabaseEntities1())
            {
                var query = (from grp in ctx.groups
                             join grpkey in ctx.group_key
                             on grp.groupNo equals grpkey.groupNo
                             where grpkey.userId == groupUserId
                             && grp.groupType == grouptype
                             && grp.groupNo != groupId
                             select new
                             {
                                 userId = grp.userId,
                                 groupname = grp.groupName,
                                 groupId = grp.groupNo,
                                 groupType = grp.groupType,
                                 groupKey = grpkey.groupKey
                             }).ToList();
                if (grouptype == 1 || grouptype == 3)
                {
                    query = query.Where(x => x.userId == groupUserId).ToList();
                }

                return query.AsQueryable();
            }


        }

        /*  public IQueryable GetGroupListforPost(int groupUserId, int grouptype)
          {

              using (var ctx = new MlaDatabaseEntities1())
              {
                  var query = (from grp in ctx.groups
                               join grpkey in ctx.group_key
                               on grp.groupNo equals grpkey.groupNo
                               where grpkey.userId == groupUserId
                               && grpkey.versionNo == 1
                               && grp.groupType == grouptype
                               //&& (groupId != null && grp.groupNo != groupId)

                               select new
                               {
                                   userId = grp.userId,
                                   groupname = grp.groupName,
                                   groupId = grp.groupNo,
                                   groupType = grp.groupType,
                                   groupKey = grpkey.groupKey
                               }).ToList();
                  if (grouptype == 1 || grouptype == 3)
                  {
                      query = query.Where(x => x.userId == groupUserId).ToList();
                  }


                  return query.AsQueryable();
              }


          }*/


    }
}
