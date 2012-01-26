using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using Microsoft.SharePoint.Utilities;

namespace SauvegardeSharepoint
{
    class Program
    {
        static string fichier = "SauvegardeSharepointLog.txt";
        static string webUrl = "http://localhost";
        static string sauveDir = @"c:\sauve";


        static void Main(string[] args)
        {

            if (args.Length == 2)
            {
                webUrl = args[0];
                sauveDir = args[1];
            }

            System.IO.Directory.CreateDirectory(sauveDir);


            using (SPWeb oWebsite = new SPSite(webUrl).OpenWeb())
            {
                //Sauvegarder oWebsite
                SPWebCollection subSites = oWebsite.Webs;
                AfficheListes(oWebsite);

                foreach (SPWeb subSite in subSites)
                {
                    AfficheListes(subSite);
                    subSite.Close();
                }
            }
            //Console.Read();
        }

        protected static void AfficheListes(SPWeb site)
        {
            //Créer dossier
            string targetSiteFolder = SPEncode.HtmlEncode(site.Title);
            //string targetFolder = site.Title;
            EcrireLog("/-------------------------------------------------------------");
            EcrireLog("/ Site ou sous-site:" + targetSiteFolder);
            EcrireLog("/-------------------------------------------------------------");
            string siteFolderPath = System.IO.Path.Combine(sauveDir, targetSiteFolder);
            System.IO.Directory.CreateDirectory(siteFolderPath);

            SPListCollection docLibList = site.GetListsOfType(SPBaseType.DocumentLibrary);
            foreach (SPList docLib in docLibList)
            {
                if (!docLib.Hidden)
                {
                    string docLibTitle = SPEncode.HtmlEncode(docLib.Title);
                    EcrireLog("-------------------------------------------------------------");
                    EcrireLog("- Biblio:" + docLibTitle + " Nb elements:" + docLib.ItemCount.ToString());
                    EcrireLog("-------------------------------------------------------------");
                    //if (oList.ItemCount > 0)
                    //{
                    SPDocumentLibrary mylib = (SPDocumentLibrary)site.Lists[docLib.Title];
                    download(mylib.RootFolder, siteFolderPath);
                    recursive(mylib.RootFolder, siteFolderPath);
                    //}
                }
            }
        }

        protected static void recursive(SPFolder dossierSP, string cheminFS)
        {
            foreach (SPFolder dossier in dossierSP.SubFolders)
            {
                download(dossier, cheminFS);
                recursive(dossier, cheminFS);
            }
        }
        //retourne un nom de dossier valide pour file system
        private static string getSafeFileName(string fileName)
        {
            string safeFileName = "";
            if (!string.IsNullOrEmpty(fileName)) safeFileName = fileName.Replace("/", "\\");
            return safeFileName;
        }
        protected static void download(SPFolder folder, string targetf)
        {
            foreach (SPFile file in folder.Files)
            {
                string message = "";
                string fileName = file.Name;
                byte[] binfile = file.OpenBinary();
                string completePath = file.ParentFolder.ToString();
                string folderPath = System.IO.Path.Combine(targetf, getSafeFileName(completePath));
                System.IO.Directory.CreateDirectory(folderPath);

                string filePath = System.IO.Path.Combine(folderPath, fileName);

                bool response = false;

                try
                {
                    FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(binfile);
                    bw.Close();
                    response = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (response == true)
                {
                    message = "Copie ok: " + fileName;// +" dans " + filePath;
                }
                else
                {
                    message = "Erreur: " + fileName;
                }

                EcrireLog(message);
            }
        }
        protected static void EcrireLog(string ligne, bool append = true)
        {
            Console.WriteLine(ligne);
            try
            {
                using (StreamWriter sw = new StreamWriter(fichier, append))
                {
                    sw.WriteLine(ligne);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
