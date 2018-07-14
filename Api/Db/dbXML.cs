
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Linq;
/*
https://github.com/kameko/MicroDB
A tiny dead-simple C# XML database in under 300 lines.
MicroDB

MicroDB is an extremely simple 280 line database written in C#. 
It is not ACID-compliant, has no encryption, 
but it is thread-safe. It has only a handfull of commands. 

    Here is an average usecase:

var DB = new Database(@".\Database"); //assumes .\Database, you can use the no-argument overload.
var context = DB.Register("MyService"); //creates directory .\Database\MyService
var file = DB.Open("MyStuff"); //creates or opens .\Database\MyService\MyStuff.xml
file.Persist("mystuffkey", "mystuffvalue"); //creates or updates mystuffkey
file.Persist("myxml", "somevalue", new XElement("xdocstoo")); //overload for XElements
var f = file.FindElement("myxml");

if (f != null && f.Inner != null)
{
    XElement newxelm = doSomethingWith(f.Inner);
    file.Persist(f.Key, f.Value, newxelm);
}

foreach (var i in file.All) //get all keys
{
    Console.WriteLine("Key {0}, Value {1}", i.Key, i.Value);
}

file.Delete("myxml");


I created this because I wanted to use an already-existing 
database library in my application, but there was a dependency 
conflict and I could not use it with the other libraries I was using. 
I ended up having to hack this together in a day, 
but to my surprise, I was very happy with this. 
Now I'm using it in all my projects that need a dead-simple database 
that doesn't require atomicity or encryption, or projects that even benefit 
from raw XML files that the end-user can edit if they wish.

I was inspired by MarcelloDB for the API (Persist/Delete), 
which was also the database I wanted to use, but couldn't, 
because I was alreadying using a library that required a 
different version of Newtonsoft.Json. 
If you do need a database with an equally simple 
API but better performance and scalability and ACID compliancy, 
I recommend you check that project out instead.

I'm releasing this as public domain, 
seeing as it's only a measily 280 lines. 
Feel free to change the license to BSD or GPL or 
whatever else you want if your project has specific 
licensing terms.
 
 */
namespace core.Db
{
    public class dbXML
    {
        public string Name { private set; get; }
        public string DBPath { private set; get; }

        public dbXML() : this(null) { }

        public dbXML(string path)
        {
            Name = "Database";
            if (path != null)
                DBPath = path;
            else
                DBPath = @".\Database";

            if (!Directory.Exists(DBPath))
            {
                Directory.CreateDirectory(DBPath);
                //log creating a directory here
            }
        }

        public Context Register(string name)
        {
            return Register(name, true);
        }

        public Context Register(string name, bool makenew)
        {
            return new Context(DBPath, name, makenew);
        }

        public class Context
        {
            public string Name { private set; get; }
            public string CPath { set; get; }

            public Context(string path, string name, bool makenew)
            {
                string fpath = string.Format(@"{0}\{1}", path, name);
                CPath = fpath;
                Name = string.Format("DatabaseContext({0})", name);

                if (!Directory.Exists(fpath) && makenew == true)
                {
                    Directory.CreateDirectory(fpath);
                    //log creating a directory here
                }
            }

            public FileContext Open(string file)
            {
                if (Directory.Exists(CPath))
                    return new FileContext(CPath, file);
                else
                    return null;
            }

            public IEnumerable<FileContext> OpenAll
            {
                get { return GetOpenAll(); }
            }

            public IEnumerable<FileContext> GetOpenAll()
            {
                string[] sa = Directory.GetFiles(CPath);
                foreach (string s in sa)
                {
                    yield return new FileContext(s, null);
                }
            }
        }

        public class Element
        {
            public string Key { set; get; }
            public string Value { set; get; }
            public XElement Inner { set; get; }
            public Element() { }
            public Element(string k, string v) : this(k, v, null) { }
            public Element(string k, string v, XElement i)
            {
                Key = k;
                Value = v;
                Inner = i;
            }
        }

        public class FileContext
        {
            private object locker = new object();
            public string Name { private set; get; }
            public string CPath { set; get; }

            public FileContext(string path, string name)
            {
                string fpath = path;

                if (name != null)
                {
                    if (!name.All(c => Char.IsLetterOrDigit(c) || c.Equals('_')))
                    {
                        name = Guid.NewGuid().ToString();
                    }
                }
                else
                {
                    name = Guid.NewGuid().ToString();
                }

                fpath = string.Format(@"{0}\{1}.xml", path, name);
                CPath = fpath;
                Name = string.Format("DatabaseFileContext({0})", name);

                if (!File.Exists(fpath))
                {
                    lock (locker)
                    {
                        using (FileStream fs = File.Create(fpath))
                        {
                            string s = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<DB />";
                            Byte[] info = new UTF8Encoding(true).GetBytes(s);
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    //log creating a directory here
                }
            }

            public Element FindElement(string pkey)
            {
                Element e = null;
                try
                {
                    foreach (var elm in GetAll())
                    {
                        if (elm.Key == pkey)
                        {
                            e = elm;
                            break;
                        }
                    }
                }
                catch
                {

                }
                return e;
            }

            public XElement FindInner(string pkey)
            {
                var p = FindElement(pkey);
                if (p != null)
                    return p.Inner;
                return null;
            }

            public string Find(string pkey)
            {
                var p = FindElement(pkey);
                if (p != null)
                    return p.Value;
                return null;
            }

            public IEnumerable<Element> All
            {
                get { return GetAll(); }
            }

            public IEnumerable<Element> GetAll()
            {
                lock (locker)
                {
                    XDocument xmlFile = XDocument.Load(CPath);
                    var query = from c in xmlFile.Elements("DB").Elements("DBReg") select c;
                    foreach (XElement elm in query)
                    {
                        Element e = new Element();
                        e.Key = elm.Attribute("key").Value;
                        e.Value = elm.Attribute("value").Value;
                        foreach (XElement es in elm.Elements())
                        {
                            e.Inner = es;
                            break;
                        }
                        yield return e;
                    }
                }
            }

            public void Persist(Element e)
            {
                Persist(e.Key, e.Value, e.Inner);
            }

            /// <summary>
            /// create or update
            /// </summary>
            /// <param name="pkey"></param>
            /// <param name="pvalue"></param>
            public void Persist(string pkey, string pvalue)
            {
                Persist(pkey, pvalue, null);
            }

            public void Persist(string pkey, string pvalue, XElement content)
            {
                lock (locker)
                {
                    XDocument xmlFile = XDocument.Load(CPath);

                    if (string.IsNullOrEmpty(Find(pkey)))
                    {
                        //create
                        xmlFile.Element("DB").Add(
                            new XElement("DBReg",
                                new XAttribute("key", pkey),
                                new XAttribute("value", pvalue),
                                content
                            )
                        );
                    }
                    else
                    {
                        //update
                        var query = from c in xmlFile.Elements("DB").Elements("DBReg") select c;
                        foreach (XElement elm in query)
                        {
                            if (elm.Attribute("key").Value == pkey)
                            {
                                elm.Attribute("value").Value = pvalue;
                                if (content != null)
                                {
                                    elm.RemoveNodes();
                                    elm.Add(content);
                                }
                            }
                        }
                    }

                    xmlFile.Save(CPath);
                }
            }

            public bool Delete(string pkey)
            {
                lock (locker)
                {
                    XDocument xmlFile = XDocument.Load(CPath);
                    //have to duplicate Find's functionality because trying to
                    //use it here seems to permanently hold the locker and then it
                    //can't delete anyting.
                    var query = from c in xmlFile.Elements("DB").Elements("DBReg") select c;
                    foreach (XElement elm in query)
                    {
                        if (elm.Attribute("key").Value == pkey)
                        {
                            xmlFile.Element("DB")
                            .Elements("DBReg")
                            .Where(x => (string)x.Attribute("key") == pkey)
                            .Remove();

                            xmlFile.Save(CPath);
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }
}