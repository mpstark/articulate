using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech;
using System.Speech.Recognition.SrgsGrammar;
using Articulate;
using YAXLib;
using System.Xml.Linq;

namespace ArticulateTest
{
    [TestFixture]
    class ProfileTests
    {
        /// <summary>
        /// THIS IS NOT A PROPER UNIT TEST AS IT GENERATES XML ONTO THE HARD DRIVE.
        /// </summary>
        [Test]
        public void TestProfileAndWriteXML()
        {
            Profile profile = new Profile();

            profile.Applications.Add("arma3.exe");
            profile.AppRequirements = Profile.ApplicationRequirements.Foreground;
            profile.IncludeFile = "fake.lua";
            profile.InitCode = "arma3 = {};";
            profile.Locale = "en";
            profile.Name = "Arma Test Profile";

            SymbolGroup groupSymbol = new SymbolGroup();
            groupSymbol.Name = "testgroup";

            Symbol testSymbol = new Symbol();
            testSymbol.Name = "test";
            testSymbol.Pronounceables.Add("test");
            testSymbol.Pronounceables.Add("testing");
            testSymbol.Pronounceables.Add("testing this thing");
            testSymbol.Value = "F1";
            groupSymbol.Members.Add(testSymbol);

            Symbol wheeSymbol = new Symbol();
            wheeSymbol.Name = "whee";
            wheeSymbol.Pronounceables.Add("weee");
            wheeSymbol.Pronounceables.Add("wheeeee");
            wheeSymbol.Pronounceables.Add("woohooo");
            wheeSymbol.Value = "F2";
            groupSymbol.Members.Add(wheeSymbol);

            profile.Symbols.Add(groupSymbol);

            NewCommand testCommand = new NewCommand();
            testCommand.Name = "testCommand";
            testCommand.Format = "(\"hello\"|\"Hi\")? (test|whee) testgroup?";
            testCommand.Output = "NotImplemented()";

            profile.Commands.Add(testCommand);

            SrgsDocument doc = profile.CompileToSrgsDocument();

            System.Xml.XmlWriter xWriter = System.Xml.XmlWriter.Create("testgrammar.xml");
            doc.WriteSrgs(xWriter);
            xWriter.Close();

            YAXSerializer serializer = new YAXSerializer(typeof(Profile));
            XDocument xdoc = serializer.SerializeToXDocument(profile);
            xdoc.Save("testprofile.xml");
        }
    }
}
