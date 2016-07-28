using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kOS;
using kOS.Safe;
using kOS.Suffixed;
using UnityEngine;
using AtmosphereAutopilot.UI;
using AtmosphereAutopilot;
using KOS_AAP;

using kOS.Safe.Encapsulation;

namespace kOS.AddOns.KOSAAP
{
    [kOSAddon("AAP")]
    [kOS.Safe.Utilities.KOSNomenclature("AAPAddon")]
    public class Addon : Suffixed.Addon
    {
        internal AapControler koscontroler = new AapControler(AtmosphereAutopilot.AtmosphereAutopilot.Instance);
        public Addon(SharedObjects shared) : base(shared)
        {
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix(new[] { "SETFLIGHTMODE", "SETFM" }, new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<BooleanValue, StringValue>(SetFlightMode, "Set the Flight Mode"));
            AddSuffix("LISTMODES", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<ListValue>(ListFlightModes, "..."));
            AddSuffix("CURRENTMODE", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetFlightMode, "..."));

            AddSuffix("MD_SETDIR", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<BooleanValue, Vector>(MDSetDir, "..."));

            AddSuffix("CC_SETMODE", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<BooleanValue, StringValue>(CCSetMode, "..."));
            AddSuffix("CC_SETWP", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<BooleanValue, GeoCoordinates>(CCSetWp, "..."));
            AddSuffix("CC_DELWP", new kOS.Safe.Encapsulation.Suffixes.NoArgsVoidSuffix(koscontroler.removeWaypoint, "..."));
            AddSuffix("CC_SETALT", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<ScalarValue>(CCSetAltitude, "..."));
            AddSuffix("CC_SETVERTICAL", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<ScalarValue>(CCSetVertical, "..."));


            //           AddSuffix("COMPLETEDSCANS", new kOS.Safe.Encapsulation.Suffixes.TwoArgsSuffix<ListValue, BodyTarget, GeoCoordinates>(GetScans, "..."));
            //           AddSuffix("ALLSCANTYPES", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetScanNames, "..."));
            //           AddSuffix("ALLRESOURCES", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<ListValue>(GetResourceNames, "..."));
            //           AddSuffix("RESOURCEAT", new kOS.Safe.Encapsulation.Suffixes.VarArgsSuffix<ScalarDoubleValue, Structure>(GetResourceByName, "..."));
            //           AddSuffix("SLOPE", new kOS.Safe.Encapsulation.Suffixes.TwoArgsSuffix<ScalarDoubleValue, BodyTarget, GeoCoordinates>(GetSlope, "..."));
            //           AddSuffix("GETCOVERAGE", new kOS.Safe.Encapsulation.Suffixes.TwoArgsSuffix<ScalarDoubleValue, BodyTarget, StringValue>(GetCoverage, "..."));
        }

        public override BooleanValue Available()
        {
            return true;
        }



        #region Listings
        public ListValue ListFlightModes()
        {
            ListValue fmnames = new ListValue();
            foreach (string fmname in Enum.GetNames(typeof(Autopilots)))
            {
                fmnames.Add(new StringValue(fmname));
            }
            return fmnames;
        }


        public StringValue GetFlightMode()
        {
            StringValue fm = new StringValue();
            fm = new StringValue(koscontroler.currentAutopilot.ToString());
            return fm;
        }

        public BooleanValue SetFlightMode(StringValue mode)
        {
            
            koscontroler.currentAutopilot = GetFmByName(mode);
            return true;
        }

        internal static Autopilots GetFmByName(string modename)
        {
            modename.ToUpper();
            Autopilots pilot = Autopilots.DISABLED;
            pilot= (Autopilots)Enum.Parse(typeof(Autopilots), modename);
            return pilot;
           }

        #endregion
        #region Mousedir
        public BooleanValue MDSetDir (Vector dir )
        {
            if (koscontroler.currentAutopilot == Autopilots.MOUSEDIR)
            {
                koscontroler.dcAP.tgt_direction = dir.ToVector3D();
                return true;
            } else
            {
                return false;
            }
        }
        #endregion
        #region CruiseControl

        internal static CruiseController.CruiseMode GetCCModeByName(string modename)
        {
            modename.ToUpper();
            CruiseController.CruiseMode mode = CruiseController.CruiseMode.LevelFlight;
            mode = (CruiseController.CruiseMode)Enum.Parse(typeof(CruiseController.CruiseMode), modename);
            return mode;
        }

        public BooleanValue CCSetMode(StringValue mode)
        {
            if (koscontroler.currentAutopilot == Autopilots.CRUISECTRL)
            {    
                koscontroler.ccAP.current_mode = GetCCModeByName(mode);
                return true;
            }
            else
            {
                return false;
            }
        }

        public BooleanValue CCSetWp (GeoCoordinates coordinate)
        {
            if (koscontroler.currentAutopilot == Autopilots.CRUISECTRL)
            {
                koscontroler.ccAP.current_waypt.latitude = coordinate.Latitude;
                koscontroler.ccAP.current_waypt.longtitude = coordinate.Longitude;
                koscontroler.ccAP.waypoint_entered = true;
                return true;
            } else
            {
                return false;
            }
        }

        public void CCSetAltitude (ScalarValue altitude)
        {
            koscontroler.ccAP.height_mode = CruiseController.HeightMode.Altitude;
            koscontroler.ccAP.desired_altitude.Value = Convert.ToSingle (altitude.Value);
            koscontroler.ccAP.vertical_control = true;
        }
        public void CCSetVertical(ScalarValue v_velocity)
        {
            koscontroler.ccAP.height_mode = CruiseController.HeightMode.VerticalSpeed;
            koscontroler.ccAP.desired_vertspeed.Value = Convert.ToSingle(v_velocity.Value);
            koscontroler.ccAP.vertical_control = true;
        }


        #endregion
    }
}