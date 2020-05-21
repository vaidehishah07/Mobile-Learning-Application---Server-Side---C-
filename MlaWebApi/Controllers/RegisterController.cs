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
    public class RegisterController : ApiController
    {
        public string cfmgr = ConfigurationManager.ConnectionStrings["MlaDatabase"].ConnectionString;
        SqlConnection cnn = null;

        public IEnumerable<Register> GetAllRegister()
        {
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId,userName,userType from register", cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);
            DataSet dsDatast = new DataSet("register");
            Sqlda.Fill(dsDatast);



            foreach (DataRow row in dsDatast.Tables[0].Rows)
            {
                yield return new Register
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }

        }

        public IEnumerable<Register> GetRegisterByUserName(string userName)
        {

            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId,userName,userType from register where userName = '" + userName + "'", cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);

            DataSet dataSet = new DataSet("register");
            Sqlda.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                yield return new Register
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }
        }

        //  [Queryable]
        [HttpGet]
        public IQueryable GetRegisterAuth(string userName, string password)
        {

          
            using (var ctx = new MlaDatabaseEntities1())
            {
                var query = ctx.registers.Where(x => x.userName == userName && x.password == password)
                 .Select(reg => new
                 {
                     userId = reg.userId,
                     userType = reg.userType,
                     userName = reg.userName,
                     publicKey = reg.publicKey,

                 }).ToList();

                return query.AsQueryable();
            }


        }

        public IEnumerable<Register> GetRegisterByUserId(int userId)
        {

            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId,userName,userType from register where userId = " + userId, cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);

            DataSet dataSet = new DataSet("register");
            Sqlda.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                yield return new Register
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }
        }

        public HttpResponseMessage PostRegisterPassUpdate(string userName, string password)
        {

            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            Register register = new Register();
            register.userName = userName;
            try
            {
                SqlCommand comm = new SqlCommand("Update register set password ='" + password + "'" + " where userName = '" + userName + "'", cnn);
                //int countUpdated =comm.ExecuteNonQuery();
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                //    comm.ExecuteNonQuery();
                //  comm.Dispose();

                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.Found, register);
                cnn.Close();
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.BadRequest, register);
                cnn.Close();
                return response;
            }

        }

        public HttpResponseMessage PostRegister(register register)
        {

            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType) values('"
                    + register.userName
                    + "','" + register.password
                    + "','" + register.userType
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                var response = Request.CreateResponse<register>(System.Net.HttpStatusCode.Created, register);

                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<register>(System.Net.HttpStatusCode.BadRequest, register);
                return response;
            }

        }

        public HttpResponseMessage PostAddInstructor(string instUserName, string instPassword, string instFirsName, string instLastName, string instTelephone, string instAddress, string instAliasMailId, string instEmailId, string instSkypeId)
        {
            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "instructor"; // userType = instructor or student or admin
            int userId = 0;

            //first add to register table then to the instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType) values('"
                    + instUserName
                    + "','" + instPassword
                    + "','" + userType
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                // retrive the userId since it is auto incremented in the database and need to be added to the instructor table
                comm = new SqlCommand("select userId from register where userName = '" + instUserName + "'", cnn);
                sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);


                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"]));
                }

            }
            catch (Exception e)
            {
                Instructor emptyInst = new Instructor();
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.BadRequest, emptyInst);
                return response;
            }

            Instructor inst = new Instructor();
            inst.idInstructor = instUserName;
            inst.firstName = instFirsName;
            inst.lastName = instLastName;
            inst.userId = userId;
            inst.telephone = instTelephone;
            inst.address = instAddress;
            inst.aliasMailId = instAliasMailId;
            inst.emailId = instEmailId;
            inst.skypeId = instSkypeId;
            // now add to instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into instructor(idInstructor,firstName,lastName,userId,telephone,address,aliasMailId,emailId, skypeId) values('"
                    + inst.idInstructor
                    + "','" + inst.firstName
                    + "','" + inst.lastName
                    + "','" + inst.userId
                    + "','" + inst.telephone
                    + "','" + inst.address
                    + "','" + inst.aliasMailId
                    + "','" + inst.emailId
                    + "','" + inst.skypeId
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                cnn.Close();
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.Created, inst);
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.BadRequest, inst);
                cnn.Close();
                return response;
            }
        }

       
        public HttpResponseMessage PostAddedofStudent(string userName, string password, string firstName, string lastName, string telephone, string address, string aliasMailId, string emailId, string skypeId)
        {
            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "student"; // userType = instructor or student or admin
            int userId = 0;

            //first add to register table then to the student table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType) values('"
                    + userName
                    + "','" + password
                    + "','" + userType
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                // retrive the userId since it is auto incremented in the database and need to be added to the student table
                comm = new SqlCommand("select userId from register where userName = '" + userName + "'", cnn);
                sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);


                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"]));
                }

            }
            catch (Exception e)
            {
                Student emptyStud = new Student();
                var response = Request.CreateResponse<Student>(System.Net.HttpStatusCode.BadRequest, emptyStud);
                cnn.Close();
                return response;
            }

            Student stud = new Student();
            stud.idStudent = userName;
            stud.firstName = firstName;
            stud.lastName = lastName;
            stud.userId = userId;
            stud.telephone = telephone;
            stud.address = address;
            stud.aliasMailId = aliasMailId;
            stud.emailId = emailId;
            stud.skypeId = skypeId;
            // now add to instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into student(idStudent,firstName,lastName,userId,telephone,address,aliasMailId,emailId,skypeId) values('"
                    + stud.idStudent
                    + "','" + stud.firstName
                    + "','" + stud.lastName
                    + "','" + stud.userId
                    + "','" + stud.telephone
                    + "','" + stud.address
                    + "','" + stud.aliasMailId
                    + "','" + stud.emailId
                    + "','" + stud.skypeId
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                cnn.Close();
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Created, "Success");
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.BadRequest, "unsuccessful");
                cnn.Close();
                return response;
            }
        }

        public HttpResponseMessage PostAddAdmin(string adminUserName, string adminPassword, string adminFirsName, string adminLastName, string adminTelephone, string adminAddress, string adminAliasMailId, string adminEmailId, string adminSkypeId)
        {
          
            DataSet dsData = new DataSet("admin");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "admin";
            int userId = 0;
            

            try
            {
                using (var ctx = new  MlaDatabaseEntities1())
                {
                   
                    register objregister = new register
                    {
                        userName = adminUserName,
                        userType = userType,
                       //password = encryptedString
                        password = adminPassword

                    };
                    ctx.registers.Add(objregister);
                    ctx.SaveChanges();
                    //var user = ctx.registers.Where(x => x.userName == adminUserName)
                    //       .Select(reg => new
                    //       {
                    //           userId = reg.userId
                    //       }).ToList();
                    admin adm = new admin();
                    adm.idAdmin = adminUserName;
                    adm.firstName = adminFirsName;
                    adm.lastName = adminLastName;
                    adm.userId = objregister.userId;
                    adm.telephone = adminTelephone;
                    adm.address = adminAddress;
                    adm.aliasMailId = adminAliasMailId;
                    adm.emailId = adminEmailId;
                    adm.skypeId = adminSkypeId;
                    ctx.admins.Add(adm);
                    ctx.SaveChanges();

                    var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Accepted, "Success");
                    return response;
                    // return query.AsQueryable();
                }

                
            }
            catch (SqlException e)
            {
                // MessageBox.Show(e.Message);
                Student emptyAdmin = new Student();
                var response = Request.CreateResponse<Student>(System.Net.HttpStatusCode.BadRequest, emptyAdmin);
                cnn.Close();
                return response;
            }


        }


        public HttpResponseMessage AddPublickey(int userId, string groupKeyDef, string publicKey, string privategroupKey)
        {

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    using (var ctx = new MlaDatabaseEntities1())
                    {
                        var objreg = (from reg in ctx.registers
                                      where reg.userId == userId
                                      select reg).SingleOrDefault();
                                                      objreg.publicKey = publicKey;

                        group grp = new group();
                        grp.groupName = "UserDef";
                        grp.groupType = 1;
                        grp.userId = userId;
                        ctx.groups.Add(grp);
                        ctx.SaveChanges();

                        group_key grpKey = new group_key();
                        grpKey.userId = userId;
                        grpKey.groupNo = grp.groupNo;
                        grpKey.groupKey = groupKeyDef;
               
                        ctx.group_key.Add(grpKey);
                        ctx.SaveChanges();

                        grp = new group();
                        grp.groupName = "Private";
                        grp.groupType = 3;
                        grp.userId = userId;

                        ctx.groups.Add(grp);
                        ctx.SaveChanges();

                        grpKey = new group_key();
                        grpKey.userId = userId;
                        grpKey.groupNo = grp.groupNo;
                        grpKey.groupKey = privategroupKey;
                        ctx.group_key.Add(grpKey);
                        ctx.SaveChanges();

                    }
                    scope.Complete();
                }

                    var response = Request.CreateResponse<string>(System.Net.HttpStatusCode.Accepted, "Success");
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