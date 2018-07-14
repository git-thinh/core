/*
JsonCF is lighweight and easy to use.It can be used from both the Compact Framework(v2.0+) or the.NET framework (v2.0+). The source
code leverages linked-files to build assemblies which target each framework.

The quickest way to get started is by using the Serialize and Deserialize static methods which accepts an object and a string respectively:

//serialize
string json = Converter.Serialize(new User("name", "password", AccountStatus.Enabled));
//or to a file
System.Json.Converter.Serialize("out.txt", new int[]{1,2,3});

//deserialize
User user = System.Json.Converter.Deserialize<User>(json);
//or from a file
int[] values = Converter.DeserializeFormFile<int[]>("out.txt");

Overloads are available to serialize/desirialize from a stream or a file(to deserialize from a file, use the DeserializeFromFile static method)

If you don't want a field serialized, mark it with the System.NonSerializedAttribute.

If you want to also include the fields from the base-class, mark the class witht he CodeBetter.Json.SerializeIncludingBase attribute.This must
be placed on each class in the hierachy.Once a class is found that doesn't have the attribute, we stop navigating down the field.

I do want to work out a good naming strategy.For now, the static Serialize and Deserialize methods accept a last parameter called "FieldPrefix". 
When serializing, this value will be stripped from the names (_userName --> userName). While deserializing, the prefix is appending (assuming
it isn't already there).
*/

namespace System.Json.Test.Console
{
    using System;
    using System.Collections.Generic;

    internal class _demo2
    {
        private static void run()
        {

            var data = new CategoryData
            {
                process = "process",
                items = new List<object[]>
                                           {
                                               new object[]{"str1", 1, 2.2, true, null},
                                               new object[]{"str2", 8, 0, false},
                                           }
            };

            var x = Converter.Serialize(data);
            var y = Converter.Deserialize<CategoryData>(x);
            var json = Converter.Serialize(new User("name", "password", AccountStatus.Disabled), "_", ProcessValue);
            Converter.Serialize("out.txt", new[] { 1, 2, 3, -4 }, "_");
            Console.WriteLine(json);


            var user = Converter.Deserialize<User>(json, "_");
            var values = Converter.DeserializeFromFile<int[]>("out.txt", "_");
            Console.WriteLine(user.UserName);

            Console.WriteLine("Done. Press enter to exit");
            Console.ReadLine();
        }

        private static bool ProcessValue(string name, ref object value)
        {
            if (string.Compare(name, "password") == 0)
            {
                value = "secret";
            }
            return true;
        }
    }

    public class BaseUser
    {
        private int _id = 1;
    }

    [SerializeIncludingBase]
    public class User : BaseUser
    {
        private string _userName;
        private string _password;
        [NonSerialized]
        private readonly Role _role;
        private AccountStatus _status;
        private Thing _think = new Thing();
        private bool _enabled = true;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public AccountStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }
        public Role Role
        {
            get { return _role; }
        }
        public Thing Thing
        {
            get { return new Thing(); }
        }

        public User(string userName, string password, AccountStatus status)
        {
            UserName = userName;
            Password = password;
            Status = status;
            _role = new Role(DateTime.Now, "Admin", this);
        }

        private User()
        {
        }
    }

    public class Role
    {
        public Role(DateTime expires, string name, User user)
        {
            Expires = expires;
            Name = name;
            User = user;
        }

        public DateTime Expires { get; set; }

        public string Name { get; set; }

        public User User { get; set; }

        public Thing Thing
        {
            get { return new Thing(); }
        }
    }

    public class Thing
    {
        private string _name = "ABC";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }

    public enum AccountStatus : long
    {
        Enabled = 1,
        Disabled = 2,
    }


    public class CategoryData
    {
        public List<object[]> items = new List<object[]>();
        public string process = "";
    }
}
