using System.Linq.Expressions;
using System.Net.Mime;
using System.Xml;
using System.Resources;
using FilesContents;

namespace EasyFunctionBlock
{
    class MyFunctionBlock
    {
        private string ThisDirectory;
        private string? PackageName;
        private string FunctionBlockName;
        private int FunctionBlockType;

        private const string FB_NAME_KEYWORD = "__FBNAME__";
        private const string PKG_NAME_KEYWORD = "__PKGNAME__";
    
        public MyFunctionBlock(string thisDirectory, int functionBlockType, string functionBlockName)
        {
            ThisDirectory = thisDirectory;
            FunctionBlockName = functionBlockName;
            FunctionBlockType = functionBlockType;
            PackageName = Path.GetFileName(ThisDirectory);

            if (FunctionBlockType<1 || FunctionBlockType>2) throw new Exception("Exception: Unknown functionblock type.");
            if (!Directory.Exists(ThisDirectory)) throw new Exception("Exception: This directory not found.");
            if (PackageName == null) throw new Exception("Exception: Directory not found.");
        }
        public void Create()
        {
            if (FunctionBlockType == 1) CreateEnable();
            else if (FunctionBlockType == 2) CreateExecute();
        }
        private void CreateExecute()
        {
            string FileContent;
            string FileName;

            // Main file
            FileContent = ExecuteFB.MAIN_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = ExecuteFB.MAIN_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);

            // Actions file
            FileContent = ExecuteFB.ACTIONS_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = ExecuteFB.ACTIONS_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);
            
            // Types file
            FileContent = ExecuteFB.TYPES_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            FileName = ExecuteFB.TYPES_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);
            
            // Function file
            FileContent = ExecuteFB.FUNCTION_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            FileName = ExecuteFB.FUNCTION_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            if (File.Exists(FileName)) MergeFUNFiles(FileName,FileContent);
            else CreateFile(ThisDirectory,FileName,FileContent);
            
            // IEC file
            FileContent = ExecuteFB.IEC_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = Directory.GetFiles(ThisDirectory,ExecuteFB.IEC_FILE_NAME).First();
            if (FileName != null) MergeIECFiles(FileName,FileContent);
            else 
            {
                FileName = ExecuteFB.IEC_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).Replace("*","lby");
                CreateFile(ThisDirectory,FileName,FileContent);
            }
        }
        private void CreateEnable()
        {
            string FileContent;
            string? FileName;

            // Main file
            FileContent = EnableFB.MAIN_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = EnableFB.MAIN_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);

            // Actions file
            FileContent = EnableFB.ACTIONS_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = EnableFB.ACTIONS_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);
            
            // Types file
            FileContent = EnableFB.TYPES_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            FileName = EnableFB.TYPES_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            CreateFile(ThisDirectory,FileName,FileContent);
            
            // Function file
            FileContent = EnableFB.FUNCTION_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            FileName = EnableFB.FUNCTION_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
            if (File.Exists(FileName)) MergeFUNFiles(FileName,FileContent);
            else CreateFile(ThisDirectory,FileName,FileContent);
            
            // IEC file
            FileContent = EnableFB.IEC_FILE_CONTENT.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).TrimStart();
            FileName = Directory.GetFiles(ThisDirectory,EnableFB.IEC_FILE_NAME).FirstOrDefault();
            if (FileName != null) MergeIECFiles(FileName,FileContent);
            else 
            {
                FileName = EnableFB.IEC_FILE_NAME.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName).Replace("*","lby");
                CreateFile(ThisDirectory,FileName,FileContent);
            }
        }
        private void CreateFile(string path, string name, string content)
        {
            string pathAndName = Path.Combine(path,name);

            var a = File.Create(pathAndName);
            a.Close();
            
            using (StreamWriter writer = new StreamWriter(pathAndName,true)) 
                writer.Write(content);
        }
        private void MergeFUNFiles(string destPath, string content)
        {            
            using (StreamWriter writer = new StreamWriter(destPath,true)) 
                writer.Write(content);
        }
        private void MergeIECFiles(string destPath, string content)
        {
            XmlDocument TemplateXmlFile = new XmlDocument();
            XmlDocument ThisXmlFile = new XmlDocument();
            XmlNode? ThisFilesNode;
            XmlNodeList? TemplateFileNodes;

            ThisXmlFile.Load(destPath);
            ThisFilesNode = ThisXmlFile.SelectSingleNode("//*[local-name()='Files']");
            if (ThisFilesNode == null) throw new Exception("Cannot find Files node in This IEC file.");
            
            TemplateXmlFile.LoadXml(content);
            TemplateFileNodes = TemplateXmlFile.SelectNodes("//*[local-name()='File']");         
            if (TemplateFileNodes == null) throw new Exception("Cannot find File nodes in template IEC file.");

            foreach(XmlNode node in TemplateFileNodes) ThisFilesNode.AppendChild(ThisXmlFile.ImportNode(node,true));                  

            // Check for duplicates
            foreach(XmlNode node1 in ThisFilesNode)
            {
                var cnt = 0;
                foreach(XmlNode node2 in ThisFilesNode)
                {
                    if (node1.InnerText == node2.InnerText) cnt = cnt + 1;
                    if (cnt>1) ThisFilesNode.RemoveChild(node2);
                }
            }

            ThisXmlFile.Save(destPath);
        }
    }

    class Program
    {
        static void Main(string[] argv)
        {
            //Console.Clear();
            
            // Get This folder
            string? ThisDirectory;
            if (argv.Count()>=1) 
            {  
                ThisDirectory = argv[1];
                if (ThisDirectory == null || !Directory.Exists(ThisDirectory)) throw new Exception("Exception: Cannot retrieve This directory.");
            }
            else
            {
                try
                {
                    ThisDirectory = Directory.GetCurrentDirectory();
                    if (ThisDirectory == null) throw new Exception("Exception: Cannot retrieve This directory.");         
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Working directory: " + ThisDirectory);
            }

            // Get function block name
            string? FunctionBlockName;
            if (argv.Count()>=2) FunctionBlockName = argv[2];
            else
            { 
                try
                {
                    Console.WriteLine("Write the functionblock name: ");
                    FunctionBlockName = Console.ReadLine();
                    if (FunctionBlockName == null) throw new Exception("Exception: Empty functionblock name");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
            }

            // Get function block type
            int FunctionBlockType;
            if (argv.Count()>=3)
            {
                try 
                {
                    FunctionBlockType = int.Parse(argv[3]);
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                try
                {
                    Console.WriteLine("Write the functionblock type: 1)Enable 2)Executable");
                    string? answ = Console.ReadLine();
                    if (answ == null) throw new Exception("Exception: Empty functionblock type");
                    FunctionBlockType = int.Parse(answ);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
            }

            // Create MyFunctionBlock object
            MyFunctionBlock myFunctionBlock;            
            try
            {
                myFunctionBlock = new MyFunctionBlock(ThisDirectory,FunctionBlockType,FunctionBlockName);

                myFunctionBlock.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Done. Bye Bye");
        }

    }

}