using System.Linq.Expressions;
using System.Net.Mime;
using System.Xml;

namespace EasyFunctionBlock
{
    class MyFunctionBlock
    {
        private string PackageDirectory;
        private string? PackageName;
        private string FunctionBlockName;
        private int FunctionBlockType;

        private const string FB_NAME_KEYWORD = "__FBNAME__";
        private const string PKG_NAME_KEYWORD = "__PKGNAME__";
        private const string TEMPLATE_ENABLE_FILE_RELATIVE_PATH = "TemplateFiles\\Enable";
        private const string TEMPLATE_EXECUTE_FILE_RELATIVE_PATH = "TemplateFiles\\Execute";

        public MyFunctionBlock(string packageDirectory, int functionBlockType, string functionBlockName)
        {
            PackageDirectory = packageDirectory;
            FunctionBlockName = functionBlockName;
            FunctionBlockType = functionBlockType;
            PackageName = Path.GetFileName(PackageDirectory);

            if (FunctionBlockType<1 || FunctionBlockType>2) throw new Exception("Exception: Unknown functionblock type.");
            if (!Directory.Exists(PackageDirectory)) throw new Exception("Exception: Package directory not found.");
            if (PackageName == null) throw new Exception("Exception: Directory not found.");
        }

        public void Create()
        {
            string FileContent;

            // Move all template files to the library folder
            List<string> TemplateFiles = new List<string>();
            if (FunctionBlockType == 1) TemplateFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(),TEMPLATE_ENABLE_FILE_RELATIVE_PATH)).ToList();
            else TemplateFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(),TEMPLATE_EXECUTE_FILE_RELATIVE_PATH)).ToList();

            string? TemplateIECFile = TemplateFiles.FirstOrDefault( file => file.Contains("\\IEC."));
            string? TemplateFUNFile = TemplateFiles.FirstOrDefault( file => file.Contains(".fun"));
            if (TemplateIECFile == null) throw new Exception("Cannot find IEC file in template folder.");
            if (TemplateFUNFile == null) throw new Exception("Cannot find .fun file in template folder.");

            string? PackageIECFile = Directory.GetFiles(PackageDirectory).FirstOrDefault( file => file.Contains("\\IEC."));
            string? PackageFUNFile = Directory.GetFiles(PackageDirectory).FirstOrDefault( file => file.Contains(".fun"));

            XmlDocument TemplateXmlFile = new XmlDocument();
            XmlDocument PackageXmlFile = new XmlDocument();
            XmlNode? PackageFilesNode = null;
            XmlNodeList? TemplateFileNodes = null;

            if (PackageIECFile != null) 
            {   
                PackageXmlFile.Load(PackageIECFile);
                PackageFilesNode = PackageXmlFile.SelectSingleNode("//*[local-name()='Files']");
                if (PackageFilesNode == null) throw new Exception("Cannot find Files node in package IEC file.");

                TemplateXmlFile.Load(TemplateIECFile);
                TemplateFileNodes = TemplateXmlFile.SelectNodes("//*[local-name()='File']");         
                if (TemplateFileNodes == null) throw new Exception("Cannot find File nodes in template IEC file.");

                TemplateFiles.Remove(TemplateIECFile);       
            }

            string FileName;
            string NewFileName;
            string DestinationPath;
            string ReadPath;
            foreach(string file in TemplateFiles)
            {
                if(file.Contains(".fun") && PackageFUNFile != null)
                {
                    ReadPath = file;
                    DestinationPath = PackageFUNFile;
                }
                else
                {
                    // Copy file from template folder to package folder
                    FileName = Path.GetFileName(file);
                    NewFileName = FileName.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
                    DestinationPath = Path.Combine(PackageDirectory,NewFileName);
                    File.Copy(file,DestinationPath,true);

                    // Add record to .lby file
                    if (PackageIECFile != null)
                    {
                        if (TemplateFileNodes == null) throw new Exception("Cannot find File nodes in template IEC file.");
                        if (PackageFilesNode == null) throw new Exception("Cannot find Files node in package IEC file.");

                        foreach(XmlNode node in TemplateFileNodes)
                        {
                            if (node.InnerText == FileName) 
                            {
                                node.InnerText = node.InnerText.Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);
                                PackageFilesNode.AppendChild(PackageXmlFile.ImportNode(node,true));    
                            }
                        }                    

                        PackageXmlFile.Save(PackageIECFile);
                    }
                    ReadPath = DestinationPath;
                }

                // Check content of file to search keywords
                using (StreamReader reader = new StreamReader(ReadPath)) 
                    FileContent = reader.ReadToEnd().Replace(PKG_NAME_KEYWORD,PackageName).Replace(FB_NAME_KEYWORD,FunctionBlockName);

                using (StreamWriter writer = new StreamWriter(DestinationPath,DestinationPath != ReadPath)) 
                    writer.Write(FileContent);                
            }

        }
    }

    class Program
    {

        static void Main(string[] argv)
        {
            Console.Clear();

            // Get package folder
            DirectoryInfo? PackageDirectory;
            if (argv.Count()>=1) 
            {  
                PackageDirectory = Directory.GetParent(argv[1]);
                if (PackageDirectory == null) throw new Exception("Exception: Cannot retrieve package directory.");
            }
            else
            {
                try
                {
                    PackageDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
                    if (PackageDirectory == null) throw new Exception("Exception: Cannot retrieve package directory.");         
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Working directory: " + PackageDirectory.FullName);
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
                myFunctionBlock = new MyFunctionBlock(PackageDirectory.FullName,FunctionBlockType,FunctionBlockName);

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