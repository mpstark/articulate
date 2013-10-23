using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Diagnostics;
using System.Globalization;

namespace Articulate
{
    static class CommandPool
    {
        private static Subject subjectObject;
        private static Dictionary<string, Command> commandObjects;

        public static SrgsDocument BuildSrgsGrammar(CultureInfo cultureInfo)
        {
            SrgsDocument document = new SrgsDocument();
            document.Culture = cultureInfo;

            // make a new subject item and then add all of it's rules to the document
            subjectObject = new Subject();
            foreach(SrgsRule rule in subjectObject.RuleList)
            {
                document.Rules.Add(rule);
            }

            SrgsRuleRef subjectRef = new SrgsRuleRef(subjectObject.RootRule);

            // go through and add all of the commands
            commandObjects = new Dictionary<string, Command>();
            SrgsOneOf commandSet = new SrgsOneOf();

            // add just the subjects
            SrgsItem select = new SrgsItem();
            select.Add(new SrgsItem(subjectRef));
            select.Add(new SrgsSemanticInterpretationTag("out.subject=rules.subject;"));
            commandSet.Add(select);

            #region Commands
            #region Move (1)
            // return to formation (1)
            Command returnToFormation = new Command("FORMUP", new string[] { "return to formation", "form up", "fallback", "fall back", "regroup", "join up", "rally on me", "rally to me" }, new uint[] { Keys.One, Keys.One }, subjectRef);
            commandObjects.Add("FORMUP", returnToFormation);
            commandSet.Add(returnToFormation.Item);

            // advance (2)
            Command advance = new Command("ADVANCE", new string[] { "advance", "move up" }, new uint[] { Keys.One, Keys.Two }, subjectRef);
            commandObjects.Add("ADVANCE", advance);
            commandSet.Add(advance.Item);

            // stay back (3)
            Command stayBack = new Command("STAYBACK", new string[] { "stay back", "go back", "back up" }, new uint[] { Keys.One, Keys.Three }, subjectRef);
            commandObjects.Add("STAYBACK", stayBack);
            commandSet.Add(stayBack.Item);

            // flank left (4)
            Command flankLeft = new Command("FLANKLEFT", new string[] { "flank left", "go left" }, new uint[] { Keys.One, Keys.Four }, subjectRef);
            commandObjects.Add("FLANKLEFT", flankLeft);
            commandSet.Add(flankLeft.Item);

            // flank right (5)
            Command flankRight = new Command("FLANKRIGHT", new string[] { "flank right", "go right" }, new uint[] { Keys.One, Keys.Five }, subjectRef);
            commandObjects.Add("FLANKRIGHT", flankRight);
            commandSet.Add(flankRight.Item);

            // stop (6)
            Command stop = new Command("STOP", new string[] { "stop", "hold position", "halt", "stay there", "stay here" }, new uint[] { Keys.One, Keys.Six }, subjectRef);
            commandObjects.Add("STOP", stop);
            commandSet.Add(stop.Item);

            // Wait for me (7)
            Command waitForMe = new Command("WAIT", new string[] { "wait for me", "wait up", "wait" }, new uint[] { Keys.One, Keys.Seven }, subjectRef);
            commandObjects.Add("WAIT", waitForMe);
            commandSet.Add(waitForMe.Item);

            // Find cover (8)
            Command cover = new Command("COVER", new string[] { "go for cover", "look for cover", "cover", "find cover", "get to cover", "hide" }, new uint[] { Keys.One, Keys.Eight }, subjectRef);
            commandObjects.Add("COVER", cover);
            commandSet.Add(cover.Item);

            // Next waypoint (9)
            Command nextWaypoint = new Command("NEXTWAYPOINT", new string[] { "next waypoint", "go to the next waypoint" }, new uint[] { Keys.One, Keys.Nine }, subjectRef);
            commandObjects.Add("NEXTWAYPOINT", nextWaypoint);
            commandSet.Add(nextWaypoint.Item);
            #endregion

            #region Target (2)
            // open menu
            Command openTargetMenu = new Command("OPENTARGET", new string[] { "show targets", "target menu", "open target menu", "targets" }, new uint[] { Keys.Two }, subjectRef);
            commandObjects.Add("OPENTARGET", openTargetMenu);
            commandSet.Add(openTargetMenu.Item);

            // cancel target (1)
            Command cancelTarget = new Command("CANCELTARGET", new string[] { "cancel target", "cancel targets", "no target" }, new uint[] { Keys.Two, Keys.One }, subjectRef);
            commandObjects.Add("CANCELTARGET", cancelTarget);
            commandSet.Add(cancelTarget.Item);
            #endregion

            #region Engage (3)
            // open fire (1)
            Command openFire = new Command("OPENFIRE", new string[] { "open fire", "go loud", "fire at will" }, new uint[] { Keys.Three, Keys.One }, subjectRef);
            commandObjects.Add("OPENFIRE", openFire);
            commandSet.Add(openFire.Item);

            // hold fire (2)
            Command holdFire = new Command("HOLDFIRE", new string[] { "hold fire", "go quiet", "cease fire" }, new uint[] { Keys.Three, Keys.Two }, subjectRef);
            commandObjects.Add("HOLDFIRE", holdFire);
            commandSet.Add(holdFire.Item);

            // fire (3)
            Command fire = new Command("FIRE", new string[] { "fire", "take the shot" }, new uint[] { Keys.Three, Keys.Three }, subjectRef);
            commandObjects.Add("FIRE", fire);
            commandSet.Add(fire.Item);

            // engage (4)
            Command engage = new Command("ENGAGE", new string[] { "engage", "move to engage" }, new uint[] { Keys.Three, Keys.Four }, subjectRef);
            commandObjects.Add("ENGAGE", engage);
            commandSet.Add(engage.Item);

            // engage at will (5)
            Command enageAtWill = new Command("ENGAGEATWILL", new string[] { "engage at will" }, new uint[] { Keys.Three, Keys.Five }, subjectRef);
            commandObjects.Add("ENGAGEATWILL", enageAtWill);
            commandSet.Add(enageAtWill.Item);

            // disengage (6)
            Command disengage = new Command("DISENGAGE", new string[] { "disengage" }, new uint[] { Keys.Three, Keys.Six }, subjectRef);
            commandObjects.Add("DISENGAGE", disengage);
            commandSet.Add(disengage.Item);

            // scan horizon (7)
            Command scanHorizon = new Command("SCANHORIZON", new string[] { "scan horizon", "scan the horizon" }, new uint[] { Keys.Three, Keys.Seven }, subjectRef);
            commandObjects.Add("SCANHORIZON", scanHorizon);
            commandSet.Add(scanHorizon.Item);

            // watch direction (8)
            // team direct object
            DirectObject direction = new DirectObject("directionDO");
            direction.Add(new string[] { "north" }, "NORTH", new uint[] { Keys.One });
            direction.Add(new string[] { "north east" }, "NORTHEAST", new uint[] { Keys.Two });
            direction.Add(new string[] { "east" }, "EAST", new uint[] { Keys.Three });
            direction.Add(new string[] { "south east" }, "SOUTHEAST", new uint[] { Keys.Four });
            direction.Add(new string[] { "south" }, "SOUTH", new uint[] { Keys.Five });
            direction.Add(new string[] { "south west" }, "SOUTHWEST", new uint[] { Keys.Six });
            direction.Add(new string[] { "west" }, "WEST", new uint[] { Keys.Seven });
            direction.Add(new string[] { "north west" }, "NORTHWEST", new uint[] { Keys.Eight });

            foreach (SrgsRule rule in direction.RuleList)
            {
                document.Rules.Add(rule);
            }

            Command watch = new Command("WATCH", new string[] { "watch", "watch the"}, new uint[] { Keys.Three, Keys.Eight }, subjectRef, direction);
            commandObjects.Add("WATCH", watch);
            commandSet.Add(watch.Item);

            // suppressive fire (9)
            Command suppresiveFire = new Command("SUPRESS", new string[] { "supppresive fire", "surpress" }, new uint[] { Keys.Three, Keys.Nine }, subjectRef);
            commandObjects.Add("SUPRESS", suppresiveFire);
            commandSet.Add(suppresiveFire.Item);
            #endregion

            #region Mount (4)
            // open mount menu
            Command openMountMenu = new Command("OPENMOUNT", new string[] { "show mount menu", "open mount menu", "show vehicles", "mount menu", "get in vehicle" }, new uint[] { Keys.Four }, subjectRef);
            commandObjects.Add("OPENMOUNT", openMountMenu);
            commandSet.Add(openMountMenu.Item);

            // dismount (1)
            Command dismount = new Command("DISMOUNT", new string[] { "dismount", "get out" }, new uint[] { Keys.Four, Keys.One }, subjectRef);
            commandObjects.Add("DISMOUNT", dismount);
            commandSet.Add(dismount.Item);
            #endregion

            #region Action (6)
            // open menu
            Command openActionMenu = new Command("OPENACTION", new string[] { "show actions", "action menu", "perform action", "do action", "open action menu", "actions" }, new uint[] { Keys.Six }, subjectRef);
            commandObjects.Add("OPENACTION", openActionMenu);
            commandSet.Add(openActionMenu.Item);
            #endregion

            #region Combat Mode (7)
            // stealth (1)
            Command stealth = new Command("STEALTH", new string[] { "stealth", "stealthy", "stealth mode" }, new uint[] { Keys.Seven, Keys.One }, subjectRef);
            commandObjects.Add("STEALTH", stealth);
            commandSet.Add(stealth.Item);

            // combat (2)
            Command combat = new Command("COMBAT", new string[] { "combat", "danger", "combat mode" }, new uint[] { Keys.Seven, Keys.Two }, subjectRef);
            commandObjects.Add("COMBAT", combat);
            commandSet.Add(combat.Item);

            // aware (3)
			Command aware = new Command("AWARE", new string[] { "aware", "alert", "aware mode", "stay sharp", "stay frosty", "stay alert" }, new uint[] { Keys.Seven, Keys.Three }, subjectRef);
            commandObjects.Add("AWARE", aware);
            commandSet.Add(aware.Item);

            // relax (4)
            Command relax = new Command("RELAX", new string[] { "relax", "relaxed mode", "safe" }, new uint[] { Keys.Seven, Keys.Four }, subjectRef);
            commandObjects.Add("RELAX", relax);
            commandSet.Add(relax.Item);

            // stand up (6)
            Command standUp = new Command("STANDUP", new string[] { "stand up", "get up", "stand" }, new uint[] { Keys.Seven, Keys.Six }, subjectRef);
            commandObjects.Add("STANDUP", standUp);
            commandSet.Add(standUp.Item);
            
            // stay crouched (7)
            Command stayCrouched = new Command("CROUCH", new string[] { "get low", "crouch", "stay crouched", "stay low" }, new uint[] { Keys.Seven, Keys.Seven }, subjectRef);
            commandObjects.Add("CROUCH", stayCrouched);
            commandSet.Add(stayCrouched.Item);

            // go prone (8)
            Command goProne = new Command("PRONE", new string[] { "go prone", "get down", "prone", "hit the dirt", "down" }, new uint[] { Keys.Seven, Keys.Eight }, subjectRef);
            commandObjects.Add("PRONE", goProne);
            commandSet.Add(goProne.Item);

            // copy my stance (9)
            Command copyMyStance = new Command("COPYMYSTANCE", new string[] { "copy my stance", "default stance" }, new uint[] { Keys.Seven, Keys.Nine }, subjectRef);
            commandObjects.Add("COPYMYSTANCE", copyMyStance);
            commandSet.Add(copyMyStance.Item);
            #endregion

            #region Formation (8)
            // column (1)
            Command column = new Command("COLUMN", new string[] { "formation column", "form column"}, new uint[] { Keys.Eight, Keys.One }, subjectRef);
            commandObjects.Add("COLUMN", column);
            commandSet.Add(column.Item);

            // staggered column (2)
            Command staggeredColumn = new Command("STAGGEREDCOLUMN", new string[] { "formation staggered column", "form staggered column" }, new uint[] { Keys.Eight, Keys.Two }, subjectRef);
            commandObjects.Add("STAGGEREDCOLUMN", staggeredColumn);
            commandSet.Add(staggeredColumn.Item);

            // wedge (3)
            Command wedge = new Command("WEDGE", new string[] { "formation wedge", "form wedge" }, new uint[] { Keys.Eight, Keys.Three }, subjectRef);
            commandObjects.Add("WEDGE", wedge);
            commandSet.Add(wedge.Item);

            // echelon left (4)
            Command echelonLeft = new Command("ECHELONLEFT", new string[] { "formation echelon left", "form echelon left" }, new uint[] { Keys.Eight, Keys.Four }, subjectRef);
            commandObjects.Add("ECHELONLEFT", echelonLeft);
            commandSet.Add(echelonLeft.Item);

            // echelon right (5)
            Command echeloneRight = new Command("ECHELONRIGHT", new string[] { "formation echelon right", "form echelon right" }, new uint[] { Keys.Eight, Keys.Five }, subjectRef);
            commandObjects.Add("ECHELONRIGHT", echeloneRight);
            commandSet.Add(echeloneRight.Item);

            // vee (6)
            Command vee = new Command("VEE", new string[] { "formation vee", "form vee" }, new uint[] { Keys.Eight, Keys.Six }, subjectRef);
            commandObjects.Add("VEE", vee);
            commandSet.Add(vee.Item);

            // line (7)
            Command line = new Command("LINE", new string[] { "formation line", "form line" }, new uint[] { Keys.Eight, Keys.Seven }, subjectRef);
            commandObjects.Add("LINE", line);
            commandSet.Add(line.Item);

            // file (8)
            Command file = new Command("FILE", new string[] { "formation file", "form file" }, new uint[] { Keys.Eight, Keys.Eight }, subjectRef);
            commandObjects.Add("FILE", file);
            commandSet.Add(file.Item);

            // diamond (9)
            Command diamond = new Command("DIAMOND", new string[] { "formation diamond", "form diamond" }, new uint[] { Keys.Eight, Keys.Nine }, subjectRef);
            commandObjects.Add("DIAMOND", diamond);
            commandSet.Add(diamond.Item);
            #endregion

            #region Assign Team (9)
            // team direct object
            DirectObject team = new DirectObject("teamDO");
            team.Add(new string[] { "team red", "red" }, "RED", new uint[] { Keys.One });
            team.Add(new string[] { "team green", "green" }, "GREEN", new uint[] { Keys.Two });
            team.Add(new string[] { "team blue", "blue" }, "BLUE", new uint[] { Keys.Three });
            team.Add(new string[] { "team yellow", "yellow" }, "YELLOW", new uint[] { Keys.Four });
            team.Add(new string[] { "team white", "white" }, "WHITE", new uint[] { Keys.Five });

            foreach (SrgsRule rule in team.RuleList)
            {
                document.Rules.Add(rule);
            }

            Command assignTeam = new Command("ASSIGN", new string[] { "assign", "assign to", "add to", "switch to" }, new uint[] { Keys.Nine }, subjectRef, team);
            commandObjects.Add("ASSIGN", assignTeam);
            commandSet.Add(assignTeam.Item);
            #endregion

            // not implemented
            #region WW_AiMenu
            #region Infantry Commands (1)
            // heal up (1)

            // rearm (2)

            // inventory (3)

            // fire on my lead (4)

            // garrison building (5)

            // clear building (6)

            // follow target (7)
            #endregion
            
            #region WayPoints (4)
            // set waypoints (1)

            // add waypoints (2)

            // move waypoints (3)

            // delete waypoints (4)

            // wait on waypoint (5)

            // cycle waypoint (6)
            #endregion
            #endregion
            #endregion
            
            // set the root rule to 
            SrgsRule commands = new SrgsRule("commands");
            commands.Add(commandSet);

            document.Rules.Add(commands);
            document.Root = commands;

            return document;
        }
        
        public static void Execute(SemanticValue semantics)
        {
            // get the subject pieces
            List<string> subjects = parseSemanticList("subject", semantics);

            // get the command
            string command = parseSemantic("command", semantics);

            // get the directObject, if any
            string directObject = parseSemantic("directObject", semantics);
            
            if (subjects != null)
            {
                // execute subject keypresses
                foreach (string subject in subjects)
                {
                    Trace.WriteLine(subject);
                    if (subjectObject.KeyLookup.ContainsKey(subject))
                    {
                        DirectInputEmulator.SendKeyPresses(subjectObject.KeyLookup[subject], 75);
                    }
                }
            }

            if (command != null && command != "")
            {
                // execute command keypresses
                Trace.WriteLine(command);
                DirectInputEmulator.SendKeyPresses(commandObjects[command].KeyLookup[command], 75);

                // execute direct object keypresses (if needed)
                if (directObject != null && directObject != "")
                {
                    Trace.WriteLine(directObject);
                    DirectInputEmulator.SendKeyPresses(commandObjects[command].KeyLookup[directObject], 75);
                }
            }
        }

        private static string parseSemantic(string key, SemanticValue semantic)
        {
            if (semantic.ContainsKey(key))
            {
                // remove weird object Object from the ToString
                int index = semantic[key].Value.ToString().IndexOf("[object Object]");
                string result = (index < 0)
                    ? semantic[key].Value.ToString()
                    : semantic[key].Value.ToString().Remove(index, "[object Object]".Length);

                string[] resultPieces = result.Split('{', '}', ' ');

                foreach (string piece in resultPieces)
                {
                    // go through each piece of the semantic and add the non-trivial pieces to our list
                    if (piece != "")
                    {
                        // return the first valid piece, shouldn't be a list
                        return piece;
                    }
                }
            }

            return null;
        }

        private static List<string> parseSemanticList(string key, SemanticValue semantic)
        {
            if (semantic.ContainsKey(key))
            {
                List<string> list = new List<string>();

                // remove weird object Object from the ToString
                int index = semantic[key].Value.ToString().IndexOf("[object Object]");
                string result = (index < 0)
                    ? semantic[key].Value.ToString()
                    : semantic[key].Value.ToString().Remove(index, "[object Object]".Length);

                string[] resultPieces = result.Split('{', '}', ' ');

                foreach (string piece in resultPieces)
                {
                    // go through each piece of the semantic and add the non-trivial pieces to our list
                    if (piece != "")
                    {
                        list.Add(piece);
                    }
                }

                return list;
            }

            return null;
        }
    }
}
