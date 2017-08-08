
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;
using System;

namespace 省赛控制台_互联网_教育_
{
    class Program
    {
        private static Encoding encode = Encoding.Default;
        private static Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //static IPAddress ip = IPAddress.Parse("172.50.161.102");
        static IPAddress ip = IPAddress.Parse("192.168.191.1");

        static void Main(string[] args)
        {

            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 9876));  //绑定IP地址：端口  
            ServerSocket.Listen(20000);    //设定最多20000个排队连接请求  
            //tcpLister.Start();
            Console.WriteLine("控制台启动成功，监听{0}", ServerSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();

        }

        static Thread receiveThread;
        /// <summary>
        /// 监听客户端
        /// </summary>
        private static void ListenClientConnect()
        {
            while (true)
            {
                object clientSocket = ServerSocket.Accept();
                IPEndPoint clientipe = (IPEndPoint)((Socket)clientSocket).RemoteEndPoint;
                //ip = clientipe.Address.ToString();//获取用户的ip
                Console.WriteLine("接收到客户端");
                receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        /// <summary>
        /// 对接收到的内容进行处理
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void ReceiveMessage(object clientSocket)
        {
            byte[] ClientData = new byte[4*1024*1024];
            Socket myClientSocket = (Socket)clientSocket;
            IPEndPoint clientipe = (IPEndPoint)((Socket)clientSocket).RemoteEndPoint;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    //ReceiveNumber存放
                    bool IsString = true;
                    int ReceiveNumber = myClientSocket.Receive(ClientData);
                    string tmp = Encoding.UTF8.GetString(ClientData, 0, ReceiveNumber);
                    string str = Encoding.UTF8.GetString(ClientData,0,3);
                    Console.WriteLine(tmp);
                    //图片处理
                    if(str =="4.1")
                    {
                        IsString = false;
                        string QuestionID = Encoding.UTF8.GetString(ClientData, 3, 17);
                        Console.WriteLine(QuestionID);
                        ReturnSocket("4.13", clientSocket);
                        QuestionImage(clientSocket, QuestionID, "4.0");
                        str = "";
                  
                    }
                    else if(str == "4.8")
                    {
                        IsString = false;
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string QuestionID = strtmp[1];
                        string PeopleID = strtmp[2];
                        ReturnSocket("4.83", clientSocket);
                        Console.WriteLine(PeopleID);
                        AnswerImage(clientSocket, QuestionID, PeopleID, "4.7");
                        str = "";
                    }
                    else if(str == "5.1")
                    {
                        IsString = false;
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string HomeworkID = strtmp[1];
                        ReturnSocket("5.13", clientSocket);
                        Console.WriteLine(HomeworkID);
                        NewHomeworkImage(clientSocket, HomeworkID, "5.0");
                        str = "";
                    }
                    else if(str == "5.9")
                    {
                        IsString = false;
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string HomeworkID = strtmp[1];
                        string PeopleID = strtmp[2];
                        ReturnSocket("5.93", clientSocket);
                        Console.WriteLine(PeopleID);
                        SubmitHomeworkImage(clientSocket, HomeworkID, PeopleID, "5.8");
                        str = "";
                    }
                    else if(str =="8.1")
                    {
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string QuestionID = strtmp[1];
                        LookQuestionImage(clientSocket, QuestionID);
                        Console.WriteLine(8.1);
                        str = "";
                    }
                    else if(str=="8.2")
                    {
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string HomeworkID = strtmp[1];
                        LookHomeworkImage(clientSocket, HomeworkID);
                        Console.WriteLine(8.2);
                        str = "";
                    }
                    else if(str=="8.3")
                    {
                        Console.WriteLine(8.3);
                        string[] strtmp = tmp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string HomeworkID = strtmp[1];
                        string PeopleID = strtmp[2];
                        LookSubmitHomeworkImage(clientSocket, HomeworkID, PeopleID);
                        Console.WriteLine(8.3);
                        str = "";
        
                    }
                    else
                    {
                        str = Encoding.UTF8.GetString(ClientData, 0, ReceiveNumber);
                    }


                    if (string.IsNullOrEmpty(str))
                    {
              
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                        break;
                    }
                    else if (IsString)
                    {
                        Console.WriteLine(str);
                        string[] stri = str.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        string DoNumber = stri[0];
                        Console.WriteLine(DoNumber);
                        
                        switch(DoNumber)
                        {

                            //登陆注册
                            case "0.0":
                                CheckAccount(clientSocket, stri[1]);
                                break;
                            case "1.0":
                                Sign(clientSocket, stri[1], stri[2], stri[3], stri[4], stri[5], stri[6]);
                                break;
                            case "1.1":
                                StudentLogin(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "1.2":
                                EditStudenMessage(clientSocket, stri[1], stri[2], stri[3], stri[4], stri[5], stri[6]);
                                break;
                            case "1.4":
                                EditTeacherMessage(clientSocket, stri[1], stri[2], stri[3], stri[4], stri[5], stri[6], stri[6]);
                                break;

                            //班级管理
                            case "2.0":
                                CreateClass(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "2.1":
                                CreaterLookClass(clientSocket, stri[1]);
                                break;
                            case "2.2":
                                CreaterEditClass(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "2.3":
                                CreaterDeleteClass(clientSocket, stri[1], stri[2]);
                                break;
                            case "2.4":
                                CreaterCreateGroup(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
                            case "2.5":
                                LookForClassmate(clientSocket, stri[1]);
                                break;
                            case "2.6":
                                CreaterEditGroup(clientSocket, stri[1], stri[2], stri[3], stri[4], stri[5]);
                                break;
                            case "2.7":
                                LookForGroup(clientSocket, stri[1]);
                                break;
                            case "2.8":
                                LookForGrouper(clientSocket, stri[1], stri[2]);
                                break;

                            //班级管理
                            case "3.0":
                                LookForClassName(clientSocket, stri[1]);
                                break;
                            case "3.1":
                                LookForClassID(clientSocket, stri[1]);
                                break;
                            case "3.2":
                                ApplyJoinClass(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
                            case "3.3":
                                CreatorLookApply(clientSocket, stri[1], stri[2]);
                                break;
                            case "3.4":
                                AgreedApply(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "3.5":
                                DisagreedApply(clientSocket, stri[1]);
                                break;
                            case "3.6":
                                ApplyerLookApply(clientSocket, stri[1], stri[2]);
                                break;
                            case "3.7":
                                DeleteApply(clientSocket, stri[1]);
                                break;
                            case "3.8":
                                LeavelClass(clientSocket, stri[1], stri[2]);
                                break;
                            case "3.9":
                                LookMyClass(clientSocket, stri[1]);
                                break;

                            //问题管理
                            case "4.0":
                                NewQuestion(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
                            case "4.2":
                                LookQuestion(clientSocket, stri[1],stri[2]);
                                break;
                            case "4.3":
                                LookQuestionMessage(clientSocket, stri[1]);
                                break;
                            case "4.4":
                                EidtMyQuestion(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "4.5":
                                DeleteQuestion(clientSocket, stri[1], stri[2]);
                                break;
                            case "4.6":
                                LookQuestionFromClassID(clientSocket, stri[1], stri[2]);
                                break;
                            case "4.7":
                                AnswerQuestion(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
                            case "4.9":
                                LookAnswerMessage(clientSocket, stri[1]);
                                break;
                            case "4.94":
                                Click_A_Like(clientSocket, stri[1]);
                                break;
                            case  "4.95":
                                Delete_A_Like(clientSocket, stri[1]);
                                break;
                            case "4.96":
                                LookAnswer(clientSocket, stri[1], stri[2]);
                                break;

                                //作业管理
                            case "5.0":
                                NewHomework(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
                            case "5.2":
                                LookHomeworkByMe(clientSocket, stri[1], stri[2]);
                                break;
                            case "5.3":
                                EidtMyHomework(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "5.4":
                                DeleteHomework(clientSocket, stri[1], stri[2]);
                                break;
                            case "5.5":
                                LookSubmitHomework(clientSocket, stri[1], stri[2]);
                                break;
                            case "5.6":
                                LookSubmitHomeworkMessage(clientSocket, stri[1], stri[2]);
                                break;
                            case "5.7":
                                LookHomework(clientSocket, stri[1], stri[2]);
                                break;
                            case "5.73":
                                LookHomeworkMessage(clientSocket, stri[1]);
                                break;
                            case "5.8":
                                SubmitHomework(clientSocket, stri[1], stri[2], stri[3], stri[4]);
                                break;
             
                            case "8.5":
                                Marking(clientSocket, stri[1], stri[2], stri[3]);
                                break;


                                //杂项
                            case "6.0":
                                AnonymousAdvice(clientSocket, stri[1], stri[2]);
                                break;
                            case "6.1":
                                LeaveWork(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "6.2":
                                LookLeaveWorkByID(clientSocket, stri[1]);
                                break;
                            case "6.3":
                                DeleteLeaveWork(clientSocket, stri[1]);
                                break;
                            case "6.4":
                                LookAnonymousAdvice(clientSocket, stri[1], stri[2]);
                                break;
                            case "6.5":
                                LookTeacher(clientSocket, stri[1]);
                                break;
                            case "6.6":
                                LookMessage(clientSocket, stri[1], stri[2],stri[3]);
                                break;
                            case "6.7":
                                NewMessage(clientSocket, stri[1], stri[2], stri[3]);
                                break;
                            case "6.9":
                                LookMessageCount(clientSocket, stri[1]);
                                break;

                            case "7.0":
                                GetQuestionName(clientSocket, stri[1]);
                                break;
                            case "7.1":
                                ForgetPassword(clientSocket, stri[1], stri[2],stri[3]);
                                break;
                            case "7.2":
                                ChangePassword(clientSocket, stri[1], stri[2]);
                                break;
                            case "7.3":
                                GetPeopleMessage(clientSocket, stri[1]);
                                break;


                            case "10.0":
                                GetTime(clientSocket);
                                break;
                            case "10.1":
                                LookLikeCount(clientSocket,stri[1]);
                                break;


                        }
                       

                    }


                }
                catch(Exception e)
                {
                    myClientSocket.Close();
                    myClientSocket.Dispose();
                    Console.WriteLine(1);
                    Console.WriteLine(e.ToString());
                    break;
                }

            }
        }

        static string Conn = "Database='database';Data Source='localhost';User Id='root';Password='960513';charset='utf8';pooling=true;Allow Zero Datetime=True";

        //注册登录
        /// <summary>
        /// 0.0:注册时验证账号是否重复
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Account"></param>
        private static void CheckAccount(object ClientSocket,string Account)
        {
            string str_sql = "SELECT * FROM 账号表 WHERE 账号 ='" + Account + "'";
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    if (tmp.HasRows)
                    {
                        ReturnSocket("0.01", ClientSocket);
                    }
                    else
                    {
                        ReturnSocket("0.02", ClientSocket);
                    }
                }
                con.Dispose();
                con.Close();
            }
            catch(Exception e)
            {
                con.Dispose();
                con.Close();
                Console.WriteLine(2);
                Console.WriteLine(e.ToString());
                ReturnSocket("0.02", ClientSocket);
            }
        }
        /// <summary>
        /// 1.0:注册
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="账号"></param>
        /// <param name="密码"></param>
        private static void Sign(object ClientSocket, string Account ,string Password,string Question,string Answer,string Name,string Identity)
        {
            //客户端IP
            //IPEndPoint Clientip = (IPEndPoint)((Socket)ClientSocket).RemoteEndPoint;
            ulong TmpID = NewID();
            string str_sql = "INSERT INTO 账号表() VALUES('" + Account + "','" + Password + "','" + Question + "','" + Answer + "','"+ Identity + "')"; //创建sql语句
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
                ReturnSocket("1.01", ClientSocket);
                con.Dispose();
                con.Close();
            }
            catch(Exception e)
            {
                con.Dispose();
                con.Close();
                Console.WriteLine(3);
                Console.WriteLine(e.ToString());
                ReturnSocket("1.02", ClientSocket);
            }
            string ins_sql = "INSERT INTO 个人信息表(个人ID,对应账号,姓名,身份,获得的赞数) VALUES('" + TmpID + "','" + Account +"','"+Name+ "','"+ Identity + "','" + 0 + "')";
            MySqlWrite(ClientSocket, ins_sql, "1.0");

        }
        /// <summary>
        /// 1.1:登陆
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Account"></param>
        /// <param name="Password"></param>
        private static void StudentLogin(object ClientSocket,string Account,string Password,string Identity)
        {
            string str_sql = "SELECT * FROM 账号表 WHERE 账号 = '" + Account + "' AND 身份 ='"+Identity+"'";
            string select_str = "SELECT * FROM 个人信息表 WHERE 对应账号 = '" + Account + "'";
            string ID = MySqlReadReturn(ClientSocket, select_str, "1.1","个人ID");
            MySqlConnection con = new MySqlConnection(Conn);
            bool Istrue = false;
            MySqlDataReader tmp;
            MySqlDataReader tmp1;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    if (tmp.HasRows)
                    {
                        tmp.Read();
                        if (tmp["密码"].ToString() == Password)
                        {
                            ReturnSocket("1.11@"+ ID , ClientSocket);
                        }
                        else
                        {
                            ReturnSocket("1.12", ClientSocket);
                        }
                    }
                    else
                    {
                        ReturnSocket("1.12", ClientSocket);
                    }
                  
                }
                con.Dispose();
                con.Close();


            }
            catch(Exception e)
            {
                Console.WriteLine(4);
                Console.WriteLine(e.ToString());
                ReturnSocket("1.12", ClientSocket);
            }
        }
        /// <summary>
        /// 1.2:第一次登陆创建个人信息
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        private static string NewStudentMessage(string Account)
        {
            ulong TmpID = NewID();
            Console.WriteLine(TmpID);
            string str_sql = "INSERT INTO 个人信息表(个人ID,对应账号) VALUES('" + TmpID + "','"+ Account+"')";
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
                return TmpID.ToString();


            }
            catch(Exception e)
            {
                Console.WriteLine(5);
                Console.WriteLine(e.ToString());
                return null;
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 1.3:修改个人信息
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID">个人ID</param>
        /// <param name="Name">个人姓名</param>
        /// <param name="School">学校</param>
        /// <param name="DateString">生日</param>
        /// <param name="Introduce">个人介绍</param>
        /// <param name="Contact">联系方式</param>
        private static void EditStudenMessage(object ClientSocket, string ID,string Name,string School,string DateString,string Introduce,string Contact)
        {
            DateTime dt = DateTime.ParseExact(DateString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            string str_sql = "UPDATE 个人信息表 SET 姓名 = '"+Name+"',学校 = '"+ School + "',生日 = '"+ dt + "',身份 = '" + "学生" + "',个人简介 = '" + Introduce + "',联系方式 = '" + Contact+"' WHERE 个人ID = '"+ ID +"'";
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql,con))
                {
                    cmd.ExecuteNonQuery();
                    ReturnSocket("1.21", ClientSocket);
                }
            }
            catch(Exception e)
            {
                ReturnSocket("1.22", ClientSocket);
                Console.WriteLine(e.ToString());
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 1.4:修改教师信息
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="School"></param>
        /// <param name="DateString"></param>
        /// <param name="Subject"></param>
        /// <param name="Introduce"></param>
        /// <param name="Contact"></param>
        private static void EditTeacherMessage(object ClientSocket, string ID, string Name, string School, string DateString,string Subject, string Introduce, string Contact)
        {
            DateTime dt = DateTime.ParseExact(DateString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            string str_sql = "UPDATE 个人信息表 SET 姓名 = '" + Name + "',学校 = '" + School + "',身份 = '教师',生日 = '" + dt + "',科目 = '" + Subject + "',个人简介 = '" + Introduce + "',联系方式 = '" + Contact + "' WHERE 个人ID = '" + ID + "'";
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    ReturnSocket("1.41", ClientSocket);
                }
            }
            catch (Exception e)
            {
                ReturnSocket("1.42", ClientSocket);
                Console.WriteLine(e.ToString());
            }
            con.Dispose();
            con.Close();
        }

        //管理班级
        /// <summary>
        /// 2.0:创建班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="Remark"></param>
        private static void CreateClass(object ClientSocket,string ID,string Name,string Remark)
        {
            ulong CreaterID = ulong.Parse(ID);
            ulong TmpID = NewID();
            Console.WriteLine(TmpID);
            string CreaterName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE  个人ID = '" + ID + "'", "2.0", "姓名");
            string str_sql = "INSERT INTO 班级表(班级ID,班级名,创建者ID,备注,创建者名) VALUES('" + TmpID + "','" + Name + "','" + CreaterID + "','" + Remark + "','" + CreaterName + "')";
            string creat_sql = "CREATE TABLE " + "class"+ TmpID + "(个人ID Bigint(20) Auto_Increment primary key,姓名 varchar(20),身份 varchar(20),小组名 varchar(20))";
            string insert_sql = "INSERT INTO class"+TmpID+ "(个人ID,姓名,身份) VALUES('" + ID + "','" + CreaterName + "','" + "老师" + "')";
            try
            {
                MySqlWriteNoReturn(ClientSocket, str_sql, "2.0");
                MySqlWrite(ClientSocket, creat_sql, "2.0");
                MySqlWrite(ClientSocket, insert_sql, "2.0");
            }
            catch(Exception e)
            {
                Console.WriteLine(6);
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// 2.1:创立者查看班级（教师端）
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID"></param>
        private static void CreaterLookClass(object ClientSocket,string ID)
        {
            string str_sql = "SELECT * FROM 班级表 WHERE 创建者ID ='" + ID + "'";
            MySqlRead(ClientSocket, str_sql, "2.1", "班级ID", "班级名", "备注");
        }
        /// <summary>
        /// 2.2:创建者编辑班级（教师端）
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="Remark"></param>
        private static void CreaterEditClass(object ClientSocket,string ID,string Name,string Remark)
        {
            string str_sql = "UPDATE 班级表 SET 班级名 = '" + Name + "',备注 = '" + Remark + "' WHERE 班级ID = '" + ID + "'";
            MySqlWrite(ClientSocket, str_sql, "2.2");
        }
        /// <summary>
        /// 2.3:创建者删除班级（教师端）
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        /// <param name="TeacherID"></param>
        private static void CreaterDeleteClass(object ClientSocket,string ClassID,string TeacherID)
        {
            string str_sql = "DELETE FROM 班级表 WHERE 班级ID =" + ClassID+" AND 创建者ID ="+TeacherID;
            string delete_sql = "DROP TABLE class"+ ClassID;
            MySqlWriteNoReturn(ClientSocket, str_sql, "2.3");
            MySqlWrite(ClientSocket, delete_sql, "2.3");

        }
        /// <summary>
        /// 2.4:创建者创建小组（教师端）
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Name"></param>
        /// <param name="GroupLeaderID"></param>
        /// <param name="GroupJoinerID"></param>  
        private static void CreaterCreateGroup(object ClientSocket,string Name,string GroupLeaderID,string GroupJoinerID,string ClassID)
        {
            //string select_sql = "SELECT 姓名 FROM class" + ClassID + "WHERE 个人ID ='" + GroupLeaderID + "'";


            string GroupLeaderName = MySqlReadReturn(ClientSocket, "SELECT * FROM class" + ClassID + " WHERE 个人ID=" + GroupLeaderID, "2.7", "姓名");
            string[] stri = GroupJoinerID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            string GroupJoinerName = MySqlReadReturn(ClientSocket, "SELECT * FROM class"+ ClassID + " WHERE 个人ID=" + stri[0], "2.7", "姓名"); ;
            for(int i=1;i<stri.Length;i++)
            {
                string tmpName = MySqlReadReturn(ClientSocket, "SELECT * FROM class" + ClassID + "  WHERE 个人ID=" + stri[i], "2.7", "姓名");
                GroupJoinerName = GroupJoinerName + "#" + tmpName;
            }
            Console.WriteLine(2333);
            string str_sql = "INSERT INTO 小组表(小组名,小组组长,小组组员,班级ID,小组长名,小组组员名) VALUES('" + Name + "','" + GroupLeaderID + "','" + GroupJoinerID + "','" + ClassID + "','" + GroupLeaderName + "','" + GroupJoinerName + "')";
            MySqlWrite(ClientSocket, str_sql, "2.4");

        }
        /// <summary>
        /// 2.5:查看班级成员
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        private static void LookForClassmate(object ClientSocket, string ClassID)
        {
            string str_sql = "SELECT * FROM class"+ ClassID;
            MySqlRead(ClientSocket, str_sql, "2.5", "个人ID", "姓名", "身份","小组名");
        }
        /// <summary>
        /// 2.6:创建者编辑小组（教师端）
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Name"></param>
        /// <param name="GroupLeaderID"></param>
        /// <param name="GroupJoinerID"></param>
        /// <param name="ClassID"></param>
        private static void CreaterEditGroup(object ClientSocket,string OldName, string NewName, string GroupLeaderID, string GroupJoinerID, string ClassID)
        {
            string GroupLeaderName = MySqlReadReturn(ClientSocket, "SELECT * FROM class" + ClassID + " WHERE 个人ID=" + GroupLeaderID, "2.7", "姓名");
            string[] stri = GroupJoinerID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            string GroupJoinerName = MySqlReadReturn(ClientSocket, "SELECT * FROM class"+ ClassID + " WHERE 个人ID=" + stri[0], "2.7", "姓名"); ;
            for(int i=1;i<stri.Length;i++)
            {
                string tmpName = MySqlReadReturn(ClientSocket, "SELECT * FROM class" + ClassID + "  WHERE 个人ID=" + stri[i], "2.7", "姓名");
                GroupJoinerName = GroupJoinerName + "#" + tmpName;
            }
            string str_sql = "UPDATE 小组表 SET 小组名 = '" + NewName + "',小组组长 = '" + GroupLeaderID + "',小组组员 = '" + GroupJoinerID + "',小组组员名 = '" + GroupJoinerName + "',小组长名 = '" + GroupJoinerName + "' WHERE 班级ID = '" + ClassID + "'AND 小组名 = '"+ OldName+"'";
            MySqlWrite(ClientSocket, str_sql, "2.6");
        }
        /// <summary>
        /// 2.7:查看班级中的小组
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Name"></param>
        /// <param name="ClassID"></param>
        private static void LookForGroup(object ClientSocket,string ClassID)
        {
            string str_sql = "SELECT * FROM 小组表 WHERE 班级ID=" + ClassID;
            MySqlRead(ClientSocket, str_sql, "2.7", "小组名","小组组长","小组组员", "小组长名","小组组员名");


        }
        /// <summary>
        /// 2.8:查看小组成员
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        /// <param name="Name"></param>
        private static void LookForGrouper(object ClientSocket, string ClassID,string Name)
        {
            string str_sql = "SELECT * FROM class"+ ClassID+" WHERE 小组名 ='" + Name +"'";
            MySqlRead(ClientSocket, str_sql, "2.8", "个人ID", "姓名", "身份");
        }


        //班级管理
        /// <summary>
        /// 3.0:通过班级名查找班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassName"></param>
        private static void LookForClassName(object ClientSocket,string ClassName)
        {
            string str_sql = "SELECT * FROM 班级表 WHERE 班级名 ='" + ClassName + "'";
            MySqlRead(ClientSocket, str_sql, "3.0", "班级ID", "班级名", "创建者ID","备注","创建者名");
        }
        /// <summary>
        /// 3.1:通过班级ID查找班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        private static void LookForClassID(object ClientSocket, string ClassID)
        {
            string str_sql = "SELECT * FROM 班级表 WHERE 班级ID ='" + ClassID + "'";
            MySqlRead(ClientSocket, str_sql, "3.1", "班级ID", "班级名", "创建者ID", "备注", "创建者名");
        }
        /// <summary>
        /// 3.2:申请加入班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ID"></param>
        /// <param name="ClassID"></param>
        /// <param name="Remark"></param>
        //Eg 3.2@111111@2016082203175691190@222222@3333333
        private static void ApplyJoinClass(object ClientSocket, string ID,string ClassID,string CreatorID,string Remark)
        {
            ulong TmpID = NewID();
            string Name = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + ID, "3.2", "姓名");
            string select_sql = "SELECT * FROM 班级表 WHERE 班级ID ='" + ClassID + "'";
            string ClassName = MySqlReadReturn(ClientSocket, select_sql, "3,2", "班级名");
            Console.WriteLine(Name + "@" + ClassName);
            string str_sql = "INSERT INTO 申请表(申请ID,申请人ID,备注,审批人ID,申请状态,申请班级ID,申请班级名,申请人名) VALUES('" + TmpID + "','" + ID + "','" + Remark + "','" + CreatorID + "','" + "待审核" + "','" + ClassID + "','" + ClassName +"','"+ Name + "')";
            MySqlWrite(ClientSocket, str_sql, "3.2");

        }
        /// <summary>
        /// 3.3:审核人查看申请
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="CreatorID"></param>
        //Eg 3.3@222222
        private static void CreatorLookApply(object ClientSocket,string CreatorID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string str_sql = "SELECT * FROM 申请表 WHERE 审批人ID ='" + CreatorID + "' ORDER BY 申请ID DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, str_sql,"3.3", "申请ID", "申请人名", "备注", "申请班级名","申请状态","申请班级ID","申请人ID");
        }
        /// <summary>
        /// 3.4:同意申请
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ApplyID"></param>
        //Eg.3.4@2016082302592510479@111111@2016082203175691190
        private static void AgreedApply(object ClientSocket,string ApplyID,string PeopleID,string ClassID)
        {
           
            string PeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'", "3.4", "姓名");
            string PeopleIdentity = MySqlReadReturn(ClientSocket, "SELECT 身份 FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'", "3.4", "身份");
            string AlreadyJoinClass = MySqlReadReturn(ClientSocket, "SELECT 班级 FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'", "3.4", "班级");
            //更新已加入的班级
            if(AlreadyJoinClass ==""|| AlreadyJoinClass == "error!")
            {
                AlreadyJoinClass = ClassID;
            }
            else
            {
                AlreadyJoinClass += "#" + ClassID;
            }
            string update_sql_class = "UPDATE 个人信息表 SET 班级 = '"+ AlreadyJoinClass + "' WHERE 个人ID ='" + PeopleID + "'";
            MySqlWriteNoReturn(ClientSocket, update_sql_class, "3.4");

            string str_sql = "UPDATE 申请表 SET 申请状态 = '通过' WHERE 申请ID ='" + ApplyID + "'";
            MySqlWriteNoReturn(ClientSocket, str_sql, "3.4");
            string insert_sql = "INSERT INTO class"+ClassID+"(个人ID,姓名,身份) VALUE('"+PeopleID+ "','" + PeopleName + "','" + PeopleIdentity + "')";
            MySqlWrite(ClientSocket, insert_sql, "3.4");
        }
        /// <summary>
        /// 3.5:拒绝申请
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ApplyID"></param>
        private static void DisagreedApply(object ClientSocket, string ApplyID)
        {
            string str_sql = "UPDATE 申请表 SET 申请状态 = '拒绝' WHERE 申请ID ='" + ApplyID + "'";
            MySqlWriteNoReturn(ClientSocket, str_sql, "3.5");
        }
        /// <summary>
        /// 3.6:申请人查看申请
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void ApplyerLookApply(object ClientSocket,string PeopleID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string str_sql = "SELECT * FROM 申请表 WHERE 申请人ID ='" + PeopleID + "' ORDER BY 申请ID DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, str_sql, "3.6", "申请ID", "备注", "申请班级名","申请状态");
        }
        /// <summary>
        /// 3.7:申请人取消申请
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ApplyID"></param>
        private static void DeleteApply(object ClientSocket, string ApplyID)
        {
            string delete_sql = "DELETE FROM 申请表 WHERE 申请ID =" + ApplyID;
            MySqlWrite(ClientSocket, delete_sql, "3.7");
        }
        /// <summary>
        /// 3.8:离开班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void LeavelClass(object ClientSocket,string PeopleID,string ClassID)
        {
            string delete_sql = "DELETE FROM class"+ClassID+" WHERE 个人ID =" + PeopleID;
            MySqlWrite(ClientSocket, delete_sql, "3.8");
            string AlreadyJoinClass = MySqlReadReturn(ClientSocket, "SELECT 班级 FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'", "3.8", "班级");
            string[] OneClassID = AlreadyJoinClass.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            int deletei = -1;
            for(int i = 0; i< OneClassID.Length;i++)
            {
                if(OneClassID[i]== ClassID)
                {
                    deletei = i;
                    break;
                }
            }
            string NewClassString = "";
            for (int i = 0; i < OneClassID.Length; i++)
            {
                if (i != deletei)
                {
                    if (NewClassString == "")
                    {
                        NewClassString = OneClassID[i];
                    }
                    else
                    {
                        NewClassString += "#" + OneClassID[i];
                    }
                }
            }
            string update_sql_class = "UPDATE 个人信息表 SET 班级 = '" + NewClassString + "' WHERE 个人ID ='" + PeopleID + "'";
            MySqlWriteNoReturn(ClientSocket, update_sql_class, "3.8");

        }
        /// <summary>
        /// 3.9:查看自己加入的班级
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        //Eg.3.9@111111
        private static void LookMyClass(object ClientSocket, string PeopleID)
        {
            try
            {
                string select_sql = "SELECT 班级 FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'";

                string classId = MySqlReadReturn(ClientSocket, select_sql, "3.9", "班级");
                string[] stri = classId.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                string classname;
                classId = "";
                for (int i = 0; i < stri.Length; i++)
                {
                    classname = MySqlReadReturn(ClientSocket, "SELECT 班级名 FROM 班级表 WHERE 班级ID =" + stri[i], "4.0", "班级名");
                    classId = classId + stri[i]  +"@"+ classname + "~";
                }
                classId = classId.Substring(0, classId.Length - 1);
                ReturnSocket("3.91@" + classId, ClientSocket);
            }
            catch(Exception e)
            {
                Console.WriteLine(7);
                Console.WriteLine(e.ToString());
                ReturnSocket("3.92", ClientSocket);
            }
        }

        //问题管理
        /// <summary>
        /// 4.0:新建一个问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        /// <param name="QName"></param>
        /// <param name="QContent"></param>
        /// <param name="ClassID"></param>
        private static void NewQuestion(object ClientSocket,string PeopleID,string QName,string QContent,string ClassID)
        { 
            string PeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + PeopleID, "4.0", "姓名");
            string ClassName = MySqlReadReturn(ClientSocket, "SELECT 班级名 FROM 班级表 WHERE 班级ID =" + ClassID, "4.0", "班级名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            if(PeopleName==null)
            {
                PeopleName = " ";
            }
            if (ClassName == null)
            {
                ClassName = " ";
            }

            ulong TmpID = NewID();
            string insert_sql = "INSERT INTO 问题表(问题ID,提问者ID,提问者姓名,问题名称,问题内容,问题所在班级ID,问题所在班级名,布置日期) VALUE('" + TmpID + "','" + PeopleID + "','" + PeopleName + "','" + QName + "','" + QContent + "','" + ClassID + "','" + ClassName + "','" + tmptime + "')";
            MySqlWriteNoReturn(ClientSocket, insert_sql, "4.0");
            ReturnSocket("4.01@" + TmpID.ToString(), ClientSocket);
        }
        /// <summary>
        /// 4.1:对问题的图片进行保存
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Image"></param>
        /// <param name="ReturnNumber"></param>
        private static void QuestionImage(object ClientSocket, string QuestionID, Image Img, string ReturnNumber)
        {


            try
            {
                ulong ImageID = NewID();
                Img.Save(@"C:/问题图片/" + ImageID.ToString() + ".jpg", ImageFormat.Png);
                string updeta_sql = "UPDATE 问题表 SET 问题图片ID = '"+ ImageID + "' WHERE 问题ID ='" + QuestionID + "'";
                MySqlWriteNoReturn(ClientSocket, updeta_sql, ReturnNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(8);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber, ClientSocket);
            }
        }
        private static void QuestionImage(object ClientSocket, string QuestionID, string ReturnNumber)
        {

            Socket hostSocket = (Socket)ClientSocket;
            try
            {

                ulong ImageID = NewID();
                using (FileStream tmpfs = File.Create(@"C:/问题图片/" + ImageID.ToString() + ".jpg"))
                {
                    byte[] Tmpbyte = new byte[2048];
                    int length = 0;
                    while ((length = hostSocket.Receive(Tmpbyte)) > 0)
                    {
                        Console.WriteLine(length.ToString());
                        tmpfs.Write(Tmpbyte, 0, length);
                    }
                    tmpfs.Flush();
                    tmpfs.Close();
                }
             
                //fs.Flush();
                //Bitmap Img = new Bitmap(fs);
                //fs.Close();
                //Img.Save(@"D:/问题图片/"+ ImageID .ToString()+ ".jpg", ImageFormat.Png);
                string PictureID = MySqlReadReturn(ClientSocket, "SELECT 问题图片ID FROM 问题表 WHERE  问题ID = '" + QuestionID + "'", "4.0", "问题图片ID");
                if (PictureID == "")
                {
                    PictureID = ImageID.ToString();
                }
                else
                {
                    PictureID += "#" + ImageID.ToString();
                }

                string updeta_sql = "UPDATE 问题表 SET 问题图片ID = '" + PictureID + "' WHERE 问题ID ='" + QuestionID + "'";
                MySqlWriteNoReturn(ClientSocket, updeta_sql, ReturnNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(79878);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber, ClientSocket);
            }
        }
        /// <summary>
        /// 4.2:查看自己的提出的问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void LookQuestion(object ClientSocket, string PeopleID,string PageNum)
        {
            //ORDER BY 时间 DESC LIMIT
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 问题表 WHERE  提问者ID = '" + PeopleID + "' ORDER BY 布置日期 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "4.2", "问题ID","问题名称","布置日期","提问者姓名","问题所在班级名");
        }
        /// <summary>
        /// 4.3:查看问题的具体信息//4.3:发回图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        private static void LookQuestionMessage(object ClientSocket,string QuestionID)
        {
            string select_sql = "SELECT * FROM 问题表 WHERE  问题ID = '" + QuestionID + "'";
            MySqlRead(ClientSocket, select_sql, "4.3", "问题内容");
        }
      
        
      

        /// <summary>
        /// 4.4:提出问题者编辑问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="QuestionName"></param>
        /// <param name="QuestionContent"></param>
        private static void EidtMyQuestion(object ClientSocket,string QuestionID,string QuestionName,string QuestionContent)
        {
            string str_sql = "UPDATE 问题表 SET 问题名称 = '" + QuestionName + "',问题内容 = '" + QuestionContent + "' WHERE 问题ID = '" + QuestionID + "'";
            MySqlWrite(ClientSocket, str_sql, "4.4");
        }
        /// <summary>
        /// 4.5:提出问题者删除问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="PeopleID"></param>
        private static void DeleteQuestion(object ClientSocket,string QuestionID,string PeopleID)
        {
            string delete_sql = "DELETE FROM 问题表 WHERE 问题ID =" + QuestionID +" AND 提问者ID ='" + PeopleID + "'";
            MySqlWrite(ClientSocket, delete_sql, "4.5");
        }
        /// <summary>
        /// 4.6:通过班级ID查看问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="Question"></param>
        private static void LookQuestionFromClassID(object ClientSocket,string ClassID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 问题表 WHERE  问题所在班级ID = '" + ClassID + "' ORDER BY 布置日期 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "4.6", "问题ID", "问题名称", "布置日期","提问者姓名","问题所在班级名");
        }
        /// <summary>
        /// 4.7:回答问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        /// <param name="QuestionID"></param>
        /// <param name="Anstring"></param>
        /// <param name="ClassID"></param>
        private static void AnswerQuestion(object ClientSocket,string PeopleID,string QuestionID,string Answer,string ClassID)
        {
            string PeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + PeopleID, "4.7", "姓名");
            string ClassName = MySqlReadReturn(ClientSocket, "SELECT 班级名 FROM 班级表 WHERE 班级ID =" + ClassID, "4.7", "班级名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            string TmpID = NewID().ToString();
            string insert_sql = "INSERT INTO 回答表(回答ID, 问题ID,回答者ID,回答内容,回答时间,问题所在班级ID,点赞数,回答者名,回答者所在班级名) VALUE('" + TmpID + "','" + QuestionID + "','" + PeopleID + "','" + Answer + "','" + tmptime + "','" + ClassID + "','" + 0 + "','" + PeopleName + "','" + ClassName + "')";
            MySqlWriteNoReturn(ClientSocket, insert_sql, "4.7 ");
            ReturnSocket("4.71@" + PeopleID+"@"+ QuestionID, ClientSocket);
        }
        /// <summary>
        /// 4.8:回答图片存储
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="PeopleID"></param>
        /// <param name="ReturnNumber"></param>
        private static void AnswerImage(object ClientSocket,string QuestionID,string PeopleID,string ReturnNumber)
        {

            Socket hostSocket = (Socket)ClientSocket;
            try
            {

                ulong ImageID = NewID();
                MemoryStream fs = new MemoryStream();
                byte[] Tmpbyte = new byte[2048];
                int length = 0;
                while ((length = hostSocket.Receive(Tmpbyte)) > 0)
                {
                    Console.WriteLine(length.ToString());
                    fs.Write(Tmpbyte, 0, length);
                }
                fs.Flush();
                Bitmap Img = new Bitmap(fs);
                Img.Save(@"C:/回答图片/" + ImageID.ToString() + ".jpg", ImageFormat.Png);
                string PictureID = MySqlReadReturn(ClientSocket, "SELECT 回答图片名 FROM 回答表 WHERE  问题ID = '" + QuestionID + "' AND 回答者ID='" + PeopleID +"'", ReturnNumber, "回答图片名");
                if (PictureID == "")
                {
                    PictureID = ImageID.ToString();
                }
                else
                {
                    PictureID += "#" + ImageID.ToString();
                }

                string updeta_sql = "UPDATE 回答表 SET 回答图片名 = '" + PictureID + "' WHERE 问题ID = '" + QuestionID + "' AND 回答者ID='" + PeopleID + "'";
                MySqlWriteNoReturn(ClientSocket, updeta_sql, ReturnNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(9);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber, ClientSocket);
            }
        }
        /// <summary>
        /// 4.96:查看粗略回答
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        private static void LookAnswer(object ClientSocket,string QuestionID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 回答表 WHERE  问题ID = '" + QuestionID + "' ORDER BY 回答时间 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "4.96", "回答ID","回答内容","回答者名","回答时间","点赞数");
        }
        /// <summary>
        /// 4.9:查看具体回答//4.9发回图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        private static void LookAnswerMessage(object ClientSocket,string AnswerID)
        {
            string select_sql = "SELECT * FROM 回答表 WHERE  回答ID = '" + AnswerID +"'";
            MySqlRead(ClientSocket, select_sql, "4.9", "回答内容");
            string PictureID = MySqlReadReturn(ClientSocket, select_sql, "4.9", "回答图片名");
            string[] stri = PictureID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < stri.Length; i++)
            {
                SendImage(ClientSocket, "回答图片/"+stri[i], "4.93");
            }
        }
        /// <summary>
        /// 4.94:回答点赞
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="PeopleID"></param>
        private static void Click_A_Like(object ClientSocket, string AnswerID)
        {
            Console.WriteLine(2333);
            string select_sql = "SELECT * FROM 回答表 WHERE 回答ID = '" + AnswerID + "'";
            int Like = int.Parse(MySqlReadReturn(ClientSocket, select_sql, "4.94", "点赞数"));
            Console.WriteLine(Like);
            string AnswerPeopleID = MySqlReadReturn(ClientSocket, select_sql, "4.94", "回答者ID");
            Like++;
            string update_sql = "UPDATE 回答表 SET 点赞数 = '" + Like + "'WHERE 回答ID = '" + AnswerID + "'";
            string str_count_sql = "SELECT * FROM 个人信息表 WHERE 个人ID = '" + AnswerPeopleID + "'";
            Console.WriteLine(AnswerPeopleID);
            int LikeCount = int.Parse(MySqlReadReturn(ClientSocket, str_count_sql, "4.94", "获得的赞数"));
            LikeCount++;
            string update_count_sql = "UPDATE 个人信息表 SET 获得的赞数 = '" + LikeCount + "'WHERE 个人ID = '" + AnswerPeopleID + "'";
            MySqlWrite(ClientSocket, update_sql, "4.94");
            MySqlWrite(ClientSocket, update_count_sql, "4.94");
        }
        /// <summary>
        /// 4.95:取消点赞
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        /// <param name="PeopleID"></param>
        private static void Delete_A_Like(object ClientSocket, string AnswerID)
        {
            string select_sql = "SELECT * FROM 回答表 WHERE 回答ID = '" + AnswerID + "'";
            int Like = int.Parse(MySqlReadReturn(ClientSocket, select_sql, "4.95", "点赞数"));
            string AnswerPeopleID = MySqlReadReturn(ClientSocket, select_sql, "4.95", "回答者ID");
            Like--;
            string update_sql = "UPDATE 回答表 SET 点赞数 = '" + Like + "'WHERE 回答ID = '" + AnswerID + "'";
            string str_count_sql = "SELECT * FROM 个人信息表 WHERE 个人ID = '" + AnswerPeopleID + "'";
            int LikeCount = int.Parse(MySqlReadReturn(ClientSocket, str_count_sql, "4.95", "获得的赞数"));
            LikeCount--;
            string update_count_sql = "UPDATE 个人信息表 SET 获得的赞数 = '" + LikeCount + "'WHERE 个人ID = '" + AnswerPeopleID + "'";
            MySqlWrite(ClientSocket, update_sql, "4.95");
            MySqlWrite(ClientSocket, update_count_sql, "4.95");
        }
        /// <summary>
        /// 10.1:查看获得的总赞数
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void LookLikeCount(object ClientSocket,string PeopleID)
        {
            string select_sql = "SELECT * FROM 个人信息表 WHERE 个人ID = '" + PeopleID + "'";
            MySqlRead(ClientSocket, select_sql, "10.1", "获得的赞数");
        }

        //作业管理
        /// <summary>
        /// 5.0:新建作业(教师端)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        /// <param name="HomeworkName"></param>
        /// <param name="HomeworkContent"></param>
        /// <param name="ClassID"></param>
        private static void NewHomework(object ClientSocket,string PeopleID,string HomeworkName,string HomeworkContent,string ClassID)
        {
            string PeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + PeopleID, "5.0", "姓名");
            string ClassName = MySqlReadReturn(ClientSocket, "SELECT 班级名 FROM 班级表 WHERE 班级ID =" + ClassID, "5.0", "班级名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            ulong TmpID = NewID();
            string insert_sql = "INSERT INTO 作业表(作业ID,作业名,布置时间,布置老师ID,作业内容,作业所在班级ID,作业所在班级名,布置老师姓名) VALUE('" + TmpID + "','" + HomeworkName + "','" + tmptime + "','" + PeopleID + "','" + HomeworkContent + "','" + ClassID + "','" + ClassName + "','" + PeopleName + "')";
            MySqlWriteNoReturn(ClientSocket, insert_sql, "5.0");
            ReturnSocket("5.01@" + TmpID.ToString(), ClientSocket);
        }
        /// <summary>
        /// 5.1:对作业图片进行保存
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="ReturnNumber"></param>
        private static void NewHomeworkImage(object ClientSocket,string HomeworkID,string ReturnNumber)
        {
            Socket hostSocket = (Socket)ClientSocket;
            try
            {

                ulong ImageID = NewID();
                MemoryStream fs = new MemoryStream();
                byte[] Tmpbyte = new byte[2048];
                int length = 0;
                while ((length = hostSocket.Receive(Tmpbyte)) > 0)
                {
                    Console.WriteLine(length.ToString());
                    fs.Write(Tmpbyte, 0, length);
                }
                fs.Flush();
                Bitmap Img = new Bitmap(fs);
                Img.Save(@"C:/作业图片/" + ImageID.ToString() + ".jpg", ImageFormat.Png);
                string PictureID = MySqlReadReturn(ClientSocket, "SELECT 作业图片名 FROM 作业表 WHERE  作业ID = '" + HomeworkID + "'", "5.0", "作业图片名");
                if (PictureID == "")
                {
                    PictureID = ImageID.ToString();
                }
                else
                {
                    PictureID += "#" + ImageID.ToString();
                }

                string updeta_sql = "UPDATE 作业表 SET 作业图片名 = '" + PictureID + "' WHERE 作业ID ='" + HomeworkID + "'";
                MySqlWriteNoReturn(ClientSocket, updeta_sql, ReturnNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(10);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber, ClientSocket);
            }
        }
        /// <summary>
        /// 5.2:查看自己发布的作业(教师端)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void LookHomeworkByMe(object ClientSocket,string PeopleID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 作业表 WHERE  布置老师ID = '" + PeopleID + "' ORDER BY 布置时间 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "5.2", "作业ID", "作业名", "布置时间", "布置老师姓名", "作业所在班级名");
        }
        /// <summary>
        /// 5.3:编辑自己发布的作业(教师端)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="HomeworkName"></param>
        /// <param name="HomeworkContent"></param>
        private static void EidtMyHomework(object ClientSocket,string HomeworkID,string HomeworkName,string HomeworkContent)
        {
            string str_sql = "UPDATE 作业表 SET 作业名 = '" + HomeworkName + "',作业内容 = '" + HomeworkContent + "' WHERE 作业ID = '" + HomeworkID + "'";
            MySqlWrite(ClientSocket, str_sql, "5.3");
        }
        /// <summary>
        /// 5.4：删除自己发布的作业(教师端)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        private static void DeleteHomework(object ClientSocket,string HomeworkID,string PeopleID)
        {
            string delete_sql = "DELETE FROM 作业表 WHERE 作业ID =" + HomeworkID + " AND 布置老师ID ='" + PeopleID + "'";
            MySqlWrite(ClientSocket, delete_sql, "5.4");
        }
        /// <summary>
        /// 5.5:粗略查看提交的作业(教师端)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="PeopleID"></param>
        private static void LookSubmitHomework(object ClientSocket,string HomeworkID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 提交作业表 WHERE  作业ID = '" + HomeworkID + "' ORDER BY 提交时间 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "5.5", "提交作业者ID","提交作业者名", "提交时间", "分数");
        }
        /// <summary>
        /// 5.6：提交的作业详情(教师端)//发回图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="PeopleID"></param>
        private static void LookSubmitHomeworkMessage(object ClientSocket,string HomeworkID,string PeopleID)
        {
            string select_sql = "SELECT * FROM 提交作业表 WHERE  作业ID = '" + HomeworkID + "' AND 提交作业者ID ='" + PeopleID + "'";
            MySqlRead(ClientSocket, select_sql, "5.6", "提交内容");
            //string PictureID = MySqlReadReturn(ClientSocket, select_sql, "5.6", "提交作业图片名");
            //string[] stri = PictureID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < stri.Length; i++)
            //{
            //    SendImage(ClientSocket, "提交作业图片/" + stri[i], "5.63");
            //}
        }
       
        /// <summary>
        /// 5.7:通过班级ID粗略查看作业
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        private static void LookHomework(object ClientSocket,string ClassID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 作业表 WHERE  作业所在班级ID = '" + ClassID + "' ORDER BY 布置时间 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "5.7", "作业ID", "作业名", "布置时间", "布置老师姓名", "作业所在班级名");

        }
        /// <summary>
        /// 5.73:详细查看作业//发回图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        private static void LookHomeworkMessage(object ClientSocket,string HomeworkID)
        {
            string select_sql = "SELECT * FROM 作业表 WHERE  作业ID = '" + HomeworkID + "'";
            MySqlRead(ClientSocket, select_sql, "5.73", "作业内容");
      
            
        }
       
        /// <summary>
            /// 5.8:提交作业
            /// </summary>
            /// <param name="ClientSocket"></param>
            /// <param name="PeopleID"></param>
            /// <param name="HomeworkID"></param>
            /// <param name="SubmitContent"></param>
            /// <param name="ClassID"></param>
        private static void SubmitHomework(object ClientSocket,string PeopleID,string HomeworkID,string SubmitContent,string ClassID)
        {
            string PeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + PeopleID, "5.8", "姓名");
            string ClassName = MySqlReadReturn(ClientSocket, "SELECT 班级名 FROM 班级表 WHERE 班级ID =" + ClassID, "5.8", "班级名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            string insert_sql = "INSERT INTO 提交作业表(作业ID,提交作业者ID,提交内容,提交时间,所在班级ID,分数,提交作业者名,所在班级名) VALUE('" + HomeworkID + "','" + PeopleID + "','" + SubmitContent + "','" + tmptime + "','" + ClassID + "','" + "未打分" + "','" + PeopleName + "','" + ClassName + "')";
            MySqlWriteNoReturn(ClientSocket, insert_sql, "5.8");
            ReturnSocket("5.81@" + HomeworkID + "@" + PeopleID, ClientSocket);
        }
        /// <summary>
        /// 5.9:提交作业图片处理
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="PeopleID"></param>
        /// <param name="ReturnNumber"></param>
        private static void SubmitHomeworkImage(object ClientSocket, string HomeworkID, string PeopleID, string ReturnNumber)
        {

            Socket hostSocket = (Socket)ClientSocket;
           try
           {
                ulong ImageID = NewID();

                using (FileStream tmpfs = File.Create(@"C:/travel_pictrue/" + ImageID.ToString() + ".jpg"))
                {

                    byte[] Tmpbyte = new byte[2048];
                    int length = 0;
                    while ((length = hostSocket.Receive(Tmpbyte)) > 0)
                    {
                        Console.WriteLine(length.ToString());
                        tmpfs.Write(Tmpbyte, 0, length);
                    }
                    tmpfs.Flush();
                    tmpfs.Close();
                }

                Console.WriteLine(HomeworkID);
                Console.WriteLine(PeopleID);
                string updeta_sql = "UPDATE 提交作业表 SET 提交作业图片名 = '" +ImageID.ToString()+ "' WHERE 作业ID = '" + HomeworkID + "' AND 提交作业者ID='" + PeopleID + "'";
                MySqlWrite(ClientSocket, updeta_sql, ReturnNumber);


                //string PictureID = MySqlReadReturn(ClientSocket, "SELECT 提交作业图片名 FROM 提交作业表 WHERE  作业ID = '" + HomeworkID + "' AND 提交作业者ID='" + PeopleID + "'", ReturnNumber, "提交作业图片名");
                //if (PictureID == "")
                //{
                //    PictureID = ImageID.ToString();
                //}
                //else
                //{
                //    PictureID += "#" + ImageID.ToString();
                //}
                //Console.WriteLine("@" + PictureID);

            }
            catch (Exception e)
            {
                Console.WriteLine(11);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber, ClientSocket);
            }
        }
   
        /// <summary>
        /// 8.5：给提交的作业打分
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="PeopleID"></param>
        /// <param name="Score"></param>
        private static void Marking(object ClientSocket, string HomeworkID, string PeopleID,string Score)
        {
            string str_sql = "UPDATE 提交作业表 SET 分数 = '" + Score + "'WHERE 作业ID ='" + HomeworkID + "' AND 提交作业者ID ='" + PeopleID + "'";
            MySqlWrite(ClientSocket, str_sql, "8.5");
        }


        //杂项

        /// <summary>
        /// 6.1:新建留言
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="LeaveWordPeopleID"></param>
        /// <param name="WasLeavedWordPeopleID"></param>
        /// <param name="LeaveWordContent"></param>
        private static void LeaveWork(object ClientSocket,string LeaveWordPeopleID, string WasLeavedWordPeopleID,string LeaveWordContent)
        {
            string LeaveWordPeopleName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + LeaveWordPeopleID, "6.1", "姓名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            ulong TmpID = NewID();
            string insert_sql = "INSERT INTO 留言表(留言ID,留言者ID,被留言者ID,留言内容,留言时间,留言者名) VALUE('" + TmpID + "','" + LeaveWordPeopleID + "','" + WasLeavedWordPeopleID + "','" + LeaveWordContent + "','" + tmptime + "','" + LeaveWordPeopleName +  "')";
            MySqlWrite(ClientSocket, insert_sql, "6.1");
        }
        /// <summary>
        /// 6.2:被留言者查看留言
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="WasLeavedWordPeopleID"></param>
        private static void LookLeaveWorkByID(object ClientSocket,string WasLeavedWordPeopleID)
        {
            string select_sql = "SELECT * FROM 留言表 WHERE  被留言者ID = '" + WasLeavedWordPeopleID + "'";
            MySqlRead(ClientSocket, select_sql, "6.2", "留言ID", "留言内容", "留言时间", "留言者名");
        }
        /// <summary>
        /// 6.3:被留言者删除留言
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="LeaveWordID"></param>
        private static void DeleteLeaveWork(object ClientSocket,string LeaveWordID)
        {
            string delete_sql = "DELETE FROM 留言表 WHERE 留言ID =" + LeaveWordID + "'";
            MySqlWrite(ClientSocket, delete_sql, "6.3");
        }
        /// <summary>
        /// 6.0：给老师发匿名建议
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="TeacherID"></param>
        /// <param name="Content"></param>
        private static void AnonymousAdvice(object ClientSocket,string TeacherID,string Content)
        {
            ulong TmpID = NewID();
            string insert_sql = "INSERT INTO 匿名建议表(建议ID,建议老师ID,建议内容) VALUE('" + TmpID + "','" + TeacherID + "','" + Content +  "')";
            MySqlWrite(ClientSocket, insert_sql, "6.0");

        } 
        /// <summary>
        /// 6.4:老师查看建议
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="TeacherID"></param>
        private static void LookAnonymousAdvice(object ClientSocket,string TeacherID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 匿名建议表 WHERE  建议老师ID = '" + TeacherID + "' ORDER BY 建议ID DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "6.4",  "建议内容");
        }
        /// <summary>
        /// 6.5:查看老师
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        private static void LookTeacher(object ClientSocket,string ClassID)
        {
            string select_sql = "SELECT * FROM class"+ClassID+" WHERE  身份= '老师'";
            MySqlRead(ClientSocket, select_sql, "6.5", "个人ID","姓名");
        }

        /// <summary>
        /// 6.6:查看通知
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        private static void LookMessage(object ClientSocket,string PeopleID,string PageNum,string FinalDate)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 通知表 WHERE  ";//班级ID= '" + ClassID + "'";
            string[] ClassID = MySqlReadReturn(ClientSocket, "SELECT * FROM 个人信息表 WHERE 个人ID ='" + PeopleID + "'","6.6", "班级").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ClassID.Length; i++)
            {
                select_sql+= "班级ID = '" + ClassID[i] + "' OR ";
            }
            select_sql = select_sql.Substring(0, select_sql.Length - 3);
            Console.WriteLine(select_sql);
            //"SELECT * FROM 通知表 WHERE  班级ID= '" + ClassID + "'ORDER BY 通知日期 DESC LIMIT " + TopNum + ",10";
            string update_sql = "UPDATE 个人信息表 SET 最后查看通知时间 = '" + FinalDate + "' WHERE 个人ID = '" + PeopleID + "'";
            MySqlWriteNoReturn(ClientSocket, update_sql, "6.6");
            MySqlRead(ClientSocket, select_sql, "6.6", "通知者名", "通知内容","通知日期","班级名");
        }
        /// <summary>
        /// 6.7:发布通知
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ClassID"></param>
        /// <param name="CreaterID"></param>
        private static void NewMessage(object ClientSocket,string ClassID,string CreaterID,string Content)
        {
            ulong TmpID = NewID();
            string ClassName = MySqlReadReturn(ClientSocket, "SELECT * FROM 班级表 WHERE 班级ID =" + ClassID, "6.7", "班级名");
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string CreaterName = MySqlReadReturn(ClientSocket, "SELECT 姓名 FROM 个人信息表 WHERE 个人ID =" + CreaterID, "6.7", "姓名");
            string insert_sql = "INSERT INTO 通知表(通知ID,通知者ID,通知者名,通知内容,班级ID,通知日期,班级名) VALUE('" + TmpID + "','" + CreaterID + "','" + CreaterName + "','" + Content + "','" + ClassID + "','" + tmptime + "','" + ClassName + "')";
            MySqlWrite(ClientSocket, insert_sql, "6.7");
        }
        /// <summary>
        /// 6.8:查看自己发布的通知
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="CreaterID"></param>
        private static void LookMyMessage(object ClientSocket,string CreaterID, string PageNum)
        {
            int TopNum = int.Parse(PageNum) * 10;
            string select_sql = "SELECT * FROM 通知表 WHERE  通知者ID= '" + CreaterID + "'ORDER BY 通知日期 DESC LIMIT " + TopNum + ",10";
            MySqlRead(ClientSocket, select_sql, "6.6", "通知者名", "通知内容", "通知日期");
        }
        /// <summary>
        /// 6.9:查看未查看的通知条数
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void LookMessageCount(object ClientSocket,string PeopleID)
        {
            try
            {
                string select_date_sql = "SELECT * FROM 个人信息表 WHERE 个人ID = '" + PeopleID + "'";
                string FinalDate = MySqlReadReturn(ClientSocket, select_date_sql, "6.9", "最后查看通知时间");
                string select_count_sql = "SELECT * FROM 通知表 WHERE 通知日期 > '" + FinalDate + "' AND (";
                string[] ClassID = MySqlReadReturn(ClientSocket, "SELECT * FROM 个人信息表 WHERE 个人ID ='" + PeopleID + "'", "6.6", "班级").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ClassID.Length; i++)
                {
                    select_count_sql += "班级ID = '" + ClassID[i] + "' OR ";
                } 
                select_count_sql = select_count_sql.Substring(0, select_count_sql.Length - 3) + ")";
                int Count = MySqlReadCount(ClientSocket, select_count_sql, "6.9");
                ReturnSocket("6.91@" + Count.ToString(), ClientSocket);
                Console.WriteLine(select_count_sql);
            }
            catch(Exception e)
            {
                ReturnSocket("6.92", ClientSocket);
            }

        }
        /// <summary>
        /// 10.0:获取系统时间
        /// </summary>
        /// <param name="ClientSocket"></param>
        private static void GetTime(object ClientSocket)
        {
            try
            {
                string tmptime = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                ReturnSocket("10.01@" + tmptime, ClientSocket);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                ReturnSocket("10.02", ClientSocket);
            }
        }
        //7.x

        /// <summary>
        /// 7.0:获取验证问题
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Account"></param>
        private static void GetQuestionName(object ClientSocket,string Account)
        {
            string str_sql = "SELECT * FROM 账号表 WHERE 账号 ='" + Account + "'";
            MySqlRead(ClientSocket, str_sql, "7.0", "密保问题");
        }
        /// <summary>
        /// 7.1:通过密保问题修改密码
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Account"></param>
        /// <param name="NewPassword"></param>
        /// <param name="QuestionName"></param>
        /// <param name="QuestionAnswer"></param>
        private static void ForgetPassword(object ClientSocket, string Account, string QuestionAnswer, string NewPassword)
        {
            string str_sql = "UPDATE 账号表 SET 密码 = '" + NewPassword + "' WHERE 账号 = '" + Account  + "' AND 密保答案 = '"+ QuestionAnswer +"'";
            MySqlWrite(ClientSocket, str_sql, "7.1");
        }
        /// <summary>
        /// 7.2:修改密码
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Account"></param>
        /// <param name="NewPassword"></param>
        private static void ChangePassword(object ClientSocket,string Account,string NewPassword)
        {
            string str_sql = "UPDATE 账号表 SET 密码 = '" + NewPassword + "' WHERE 账号 = '" + Account + "'";
            MySqlWrite(ClientSocket, str_sql, "7.2");
        }
        /// <summary>
        /// 7.3：获取个人信息
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PeopleID"></param>
        private static void GetPeopleMessage(object ClientSocket,string PeopleID)
        {
            string select_sql = "SELECT * FROM 个人信息表 WHERE  个人ID = '" + PeopleID + "'";
            MySqlRead(ClientSocket, select_sql, "7.3", "姓名", "学校", "生日", "个人简介", "联系方式");

        }


        /// <summary>
        /// 8.1:查看问题图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="QuestionID"></param>
        private static void LookQuestionImage(object ClientSocket, string QuestionID)
        {
            string select_sql = "SELECT * FROM 问题表 WHERE  问题ID = '" + QuestionID + "'";
            string PictureID = MySqlReadReturn(ClientSocket, select_sql, "4.3", "问题图片ID");
            string[] stri = PictureID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            //ReturnSocket(stri.Length.ToString(), ClientSocket);
            Console.WriteLine(stri[0]);
            SendImage(ClientSocket, "问题图片/" + stri[0], "8.13");
            //for (int i = 0; i < stri.Length; i++)
            //{

            //}
        }
        /// <summary>
        /// 8.2:查看作业图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        private static void LookHomeworkImage(object ClientSocket, string HomeworkID)
        {
            string select_sql = "SELECT * FROM 作业表 WHERE  作业ID = '" + HomeworkID + "'";
            string PictureID = MySqlReadReturn(ClientSocket, select_sql, "5.73", "作业图片名");
            string[] stri = PictureID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine("@"+PictureID);
            SendImage(ClientSocket, "作业图片/" + stri[0], "8.23");
            //for (int i = 0; i < stri.Length; i++)
            //{
            //    SendImage(ClientSocket, "作业图片/" + stri[i], "5.73");
            //}
        }
        /// <summary>
        /// 8.3:提交作业图片查看
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="HomeworkID"></param>
        /// <param name="PeopleID"></param>
        private static void LookSubmitHomeworkImage(object ClientSocket, string HomeworkID, string PeopleID)
        {
            string select_sql = "SELECT * FROM 提交作业表 WHERE  作业ID = '" + HomeworkID + "' AND 提交作业者ID ='" + PeopleID + "'";
            string PictureID = MySqlReadReturn(ClientSocket, select_sql, "5.6", "提交作业图片名");
            string[] stri = PictureID.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            SendImage(ClientSocket, "提交图片/" + stri[0], "8.33");

            //for (int i = 0; i < stri.Length; i++)
            //{
            //    SendImage(ClientSocket, "提交作业图片/" + stri[i], "5.63");
            //}
        }



        //操作函数
        /// <summary>
        /// 发回图片
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="PictureID"></param>
        private static void SendImage(object ClientSocket,string PictureName,string ReturnNumber)
        {
            Socket ReturnSocket = (Socket)ClientSocket;
            System.IO.FileStream fs = new System.IO.FileStream(@"C:/"+ PictureName + ".jpg", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
            byte[] fssize = new byte[fs.Length];
            fs.Read(fssize, 0, fssize.Length);
            fs.Close();
            Console.WriteLine(fssize.Length);
            ReturnSocket.Send(Encoding.UTF8.GetBytes(ReturnNumber+ fssize.Length.ToString()));
            ReturnSocket.Send(fssize);
        }
      

        /// <summary>
        /// 发回信息
        /// </summary>
        private static void ReturnSocket(string ReturnString, object clientSocket/*, IPEndPoint ip1*/)
        {
            Console.WriteLine(ReturnString);
            Console.WriteLine(ReturnString.Length);
            Console.WriteLine(Encoding.UTF8.GetBytes(ReturnString).Length);
            Socket myClientSocket = (Socket)clientSocket;
            myClientSocket.Send(Encoding.UTF8.GetBytes(ReturnString));

        }
        /// <summary>
        /// 随机生成ID
        /// </summary>
        /// <returns></returns>
        private static ulong NewID()
        {
            string tmptime = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmssfff");
            Random tmprandom = new Random();
            int RandomNumber = tmprandom.Next(10,99);
            tmptime =tmptime.Substring(2, tmptime.Length - 2) + RandomNumber.ToString();
            return ulong.Parse(tmptime);
        }
        /// <summary>
        /// 对数据库进行写，删操作
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        private static void MySqlWrite(object ClientSocket,string str_sql,string ReturnNumber)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    ReturnSocket(ReturnNumber+"1", ClientSocket);
                }
            }
            catch(Exception e)
            {
                ReturnSocket(ReturnNumber + "2", ClientSocket);
                Console.WriteLine(12);
                Console.WriteLine(e.ToString());
            }

            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行写，删操作(无反馈值)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        private static void MySqlWriteNoReturn(object ClientSocket, string str_sql, string ReturnNumber)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                ReturnSocket(ReturnNumber + "2", ClientSocket);
                Console.WriteLine(13);
                Console.WriteLine(e.ToString());
            }

            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(1)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "~";
                        }
                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }

                    ReturnSocket(returnstring, ClientSocket);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(14);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(2)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1, string tmp2)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "~";
                        }
                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
                    ReturnSocket(returnstring, ClientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(15);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(3)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket,string str_sql, string ReturnNumber,string tmp1,string tmp2,string tmp3)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "@" + tmp[tmp3] + "~";
                        }
                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                        ReturnSocket(returnstring, ClientSocket);
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(16);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(4)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1, string tmp2, string tmp3, string tmp4)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "@" + tmp[tmp3] + "@" + tmp[tmp4] + "~";
                        }

                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
            
                    ReturnSocket(returnstring, ClientSocket);
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(17);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(5)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1, string tmp2, string tmp3, string tmp4,string tmp5)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "@" + tmp[tmp3] + "@" + tmp[tmp4] + "@" + tmp[tmp5] + "~";
                        }

                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
                    ReturnSocket(returnstring, ClientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(18);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(6)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1, string tmp2, string tmp3, string tmp4, string tmp5,string tmp6)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "@" + tmp[tmp3] + "@" + tmp[tmp4] + "@" + tmp[tmp5] + "@" + tmp[tmp6] + "~";
                        }

                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
                    ReturnSocket(returnstring, ClientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(18);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 对数据库进行读操作(7)
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <param name="tmp2"></param>
        /// <param name="tmp3"></param>
        private static void MySqlRead(object ClientSocket, string str_sql, string ReturnNumber, string tmp1, string tmp2, string tmp3, string tmp4, string tmp5, string tmp6,string tmp7)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    string returnstring = "";
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            returnstring += tmp[tmp1] + "@" + tmp[tmp2] + "@" + tmp[tmp3] + "@" + tmp[tmp4] + "@" + tmp[tmp5] + "@" + tmp[tmp6] + "@" + tmp[tmp7] + "~";
                        }

                        returnstring = returnstring.Substring(0, returnstring.Length - 1);
                        returnstring = ReturnNumber + "1@" + returnstring;
                    }
                    else
                    {
                        returnstring = ReturnNumber + "1";
                    }
                    ReturnSocket(returnstring, ClientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(18);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
            }
            con.Dispose();
            con.Close();
        }
        /// <summary>
        /// 返回数据条数
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <returns></returns>
        private static int MySqlReadCount(object ClientSocket ,string str_sql ,string ReturnNumber)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            int ReturnCount = 0;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    if (tmp.HasRows)
                    {
                        while (tmp.Read())
                        {
                            ReturnCount++;
                        }

                    }
                    con.Dispose();
                    con.Close();
                    return ReturnCount;


                }
            }
            catch (Exception e)
            {
                con.Dispose();
                con.Close();
                Console.WriteLine(14);
                Console.WriteLine(e.ToString());
                ReturnSocket(ReturnNumber + "2", ClientSocket);
                return 0;
            }
        }

        /// <summary>
        /// 中途对数据库进行查找
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="str_sql"></param>
        /// <param name="ReturnNumber"></param>
        /// <param name="tmp1"></param>
        /// <returns></returns>
        private static string MySqlReadReturn(object ClientSocket, string str_sql, string ReturnNumber, string tmp1)
        {
            MySqlConnection con = new MySqlConnection(Conn);
            MySqlDataReader tmp;
            con.Open();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(str_sql, con))
                {
                    cmd.ExecuteNonQuery();
                    tmp = cmd.ExecuteReader();
                    tmp.Read();
                    if (tmp.HasRows)
                    {
                        string ReturnString = tmp[tmp1].ToString();
                        con.Dispose();
                        con.Close();
                        return ReturnString;
                    }
                    else
                    {
                        con.Dispose();
                        con.Close();
                        return null;
                    }
                }
        
            }
            catch (Exception e)
            {
                Console.WriteLine(19);
                Console.WriteLine(e.ToString());
                con.Dispose();
                con.Close();
                ReturnSocket(ReturnNumber+"2", ClientSocket);
                return "error!";
            }

           

        }
        /// <summary>
        /// 将base64字符串转成图
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static Bitmap FromBase64String(string base64Str)
        {
            Bitmap bitmap = null;
            Image img = null;
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = Convert.FromBase64String(base64Str);
                ms.Write(buffer, 0, buffer.Length);
                try
                {
                    img = Image.FromStream(ms);
                    if (img != null)
                    {
                        bitmap = new Bitmap(img.Width, img.Height);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawImage(img, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(20);
                    Console.WriteLine(e.ToString());
                }
            }
            return bitmap;
        }
        /// <summary>
        /// 将图片转成base64字符
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToBase64String(Image img, ImageFormat format)
        {
            if (img != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, format);
                    byte[] buffer = ms.ToArray();
                    return Convert.ToBase64String(buffer);
                }
            }
            return string.Empty;
        }
    }
}

  