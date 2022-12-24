
using System;
using System.Collections.Generic;
using System.Linq;
using BsinessEntity;
using System.Data.SqlClient;
using System.Data;


namespace DataAccessLayer
{
    public class UserDAL
    {
        Db db = new Db();

        //Non-duplication of username at the time of creation
        public bool usernameread(User a)
        {
            var q = db.Users.Where(i => i.username == a.username);
            if (q.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Create a new user
        public string Create(User a, UserGroup ug)
        {
            try
            {
                if (usernameread(a))
                {
                    a.userGroup = db.userGroups.Find(ug.id);
                    db.Users.Add(a);
                    db.SaveChanges();
                    return "ثبت کاربر جدید با موفقیت انجام شد";
                }
                else
                {
                    return "نام کاربری وارد شده تکراری است";
                }
            }
            catch (Exception e)
            {
                return "خطا در ثبت اطلاعات:\n" + e.Message;
            }
        }
        //Non-duplication of username during update
        public bool Updateusername(User a, int id)
        {
            var q1 = db.Users.Where(i => i.id == id).FirstOrDefault();
            var q = db.Users.Where(i => i.username == a.username);
            if (q1.username == a.username)
            {
                return true;
            }
            else if (q.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Update users
        public string updateUser(User a, int id,UserGroup ug)
        {
            var q = db.Users.Where(i => i.id == id).FirstOrDefault();
            try
            {
                if (q != null && Updateusername(a,id))
                {
                    q.shiftstart = a.shiftstart;
                    q.shiftEnd = a.shiftEnd;
                    q.name = a.name;
                    q.codemeli = a.codemeli;
                    q.username = a.username;
                    q.password = a.password;
                    q.RegTime = DateTime.Now;
                    q.pic = a.pic;
                    q.userGroup = db.userGroups.Find(ug.id);
                    db.SaveChanges();
                    return "ویرایش مشخصات کاربر با موفقیت انجام شد";
                }
                else if (Updateusername(a, id)== false)
                {
                    return "نام کاربری وارد شده تکراری است";
                }
                else 
                {
                    return "کاربر مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return " خطا در ویرایش اطلاعات کاربر :\n" + e.Message;
            }

        }
        //Update access level
        public bool updateUserAccess(int id , bool CanEnter, bool CanCreate, bool CanUpdate, bool CanDelete)
        {
            var q = db.userAccessRoles.Where(i => i.id == id).SingleOrDefault();
            q.CanEnter = CanEnter;
            q.CanCreate = CanCreate;
            q.CanUpdate = CanUpdate;
            q.CanDelete = CanDelete;
            db.SaveChanges();
            return true;
        }
        //Update Pass
        public string UpdatePassword(User user, int id)
        {
            try
            {
                var q = db.Users.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.password = user.password;
                    db.SaveChanges();
                    return "ویرایش رمز ورود با موفقیت انجام شد";
                }
                else
                {
                    return "کاربر مورد نظر یافت نشد";
                }
            }
            catch (Exception ex)
            {
                return "خطا در ویرایش رمز ورود\n" + ex.Message;
            }
        }
        //Deactivation of users
        public string Delete(int id)
        {
            var q = db.Users.Where(i => i.id == id).FirstOrDefault();
            try
            {
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف اطلاعات با موفقیت انجام شد";
                }
                else
                {
                    return "کاربر مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "خطا در حذف اطلاعات:\n" + e.Message;
            }

        }
        //Is the software registered?
        public bool IsRegistered(string np)
        {
            var q2 = db.cuss.Where(i => i.np == np).FirstOrDefault();
            if (q2 != null)
            {
                return true;
            }
            else
                return false;
        }
        //Correct username and password when entering the program
        public User login(string u, string p)
        {
            try
            {
                return db.Users.Include("UserGroup").Where(i => i.username == u && i.password == p).SingleOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
        // Show all users - no input
        public DataTable read()
        {
            string cmd = "SELECT   id AS آیدی, name AS نام, codemeli AS [کد ملی], username AS [نام کاربری] FROM dbo.Users WHERE(DeleteStatus = 0)";// and username != 'ADMIN')";
            SqlConnection con = new SqlConnection(clsConnectionString.constr);
            var sqladapter = new SqlDataAdapter(cmd, con);
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];
            
        }
        //Search users based on name, user name, national code and dynamically using stored procedure
        public DataTable Read(string a,int index)
        {
            SqlCommand cmd = new SqlCommand();
            if (index == 0)
            {
                cmd.CommandText = "dbo.SearchUser";
            }
            else if (index == 1)
            {
                cmd.CommandText = "dbo.SearchUserUsername";
            }
            else if (index == 2)
            {
                cmd.CommandText = "dbo.SearchUserName";
            }
            else if (index == 3)
            {
                cmd.CommandText = "dbo.SearchUserCodemeli";
            }
            SqlConnection con = new SqlConnection(clsConnectionString.constr);
            cmd.Parameters.AddWithValue("@search", a);
            cmd.Connection = con;
            cmd.CommandType = CommandType.StoredProcedure;
            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = cmd;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];
        }
        //Search users by id using LINQ
        public User read(int id)
        {
            return db.Users.Include("UserGroup").Where(i => i.id == id).FirstOrDefault();
        }
        //Search users by username
        public User readusername(string s)
        {
            return db.Users.Where(i => i.username == s).SingleOrDefault();
        }
        //Show all usernames
        public List<string> readusernames()
        {
            return db.Users.Where(i => i.DeleteStatus == false).Select(i => i.username ).ToList();
        }
        //Checking the access level of each user
        public bool Access(User u, int a, string s)
        {
            UserGroup ug = db.userGroups.Include("userAccessRoles").Where(i => i.id == u.userGroup.id).SingleOrDefault();
            UserAccessRole uar = ug.userAccessRoles.Where(z => z.Section == s).FirstOrDefault();
            if (a == 1)
            {
                return uar.CanEnter;
            }
            else if (a == 2)
            {
                return uar.CanCreate;
            }
            else if (a == 3)
            {
                return uar.CanUpdate;
            }
            else
            {
                return uar.CanDelete;
            }
        }
        //The total volume that the user can transfer to the server in one day
        double sendsizeday ;
        double totaldaysize ;
        public bool PermissionsizeSend(User u ,DateTime day, double size)
        {
            sendsizeday=0;
            totaldaysize=0;

            DateTime start = new DateTime(day.Year, day.Month, day.Day, 00, 00, 00);
            DateTime end = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);

            //The volume that we are currently planning to transfer
            var q = db.Users.Where(a=> a.id == u.id).FirstOrDefault();
            var p = db.FTPFiles.Where(i => i.CreateName == u.username &&  i.RegTime > start && i.RegTime <= end).ToList();
            foreach (var item in p)
            {
                sendsizeday += item.size;
            }

            //checking the possibility of transfer
            totaldaysize = sendsizeday + size;
            if (q.TotalSizeSend> totaldaysize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Server date and time
        public DataTable time()
        {
            SqlConnection con = new SqlConnection(clsConnectionString.constr);
            SqlCommand cmd = new SqlCommand("dbo.SqlTime");
            cmd.Connection = con;
            cmd.CommandType = CommandType.StoredProcedure;
            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = cmd;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];
        }
    }
}
