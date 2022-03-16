using AAI_Repo.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace AAI_Repo.Tests
{
    public class Tests1
    {
        [SetUp]
        public void Setup()
        {
        }


        [TestCaseSource("GetCases")]
        public void ConnectCheck(InstallItem item)
        {
            if (item.URL == "") return;
            LinkChecher linkChecher = new LinkChecher();
            CheckResult result = linkChecher.CheckStart(item.URL);
            System.Console.WriteLine($"{item.ItemName}");
            Assert.AreEqual(CheckResult.Complete, result);
        }

        static InstallItem[] GetCases()
        {
            string aaiPath = $"{System.IO.Path.GetFullPath(".")}\\aai.repo";
            PreRepoFileR preRepoFileR = new PreRepoFileR(aaiPath);
            preRepoFileR.Open();
            preRepoFileR.ReadInstallItemList();
            preRepoFileR.Close();

            return InstallItemList.GetInstalItemList();
        }
    }
}