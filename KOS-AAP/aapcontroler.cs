using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtmosphereAutopilot;
using UnityEngine;

namespace KOS_AAP
{
    // shamlessly copied from AAP
    class AapControler
    {
        private AtmosphereAutopilot.AtmosphereAutopilot parent;

        internal TopModuleManager masterAP = null;
        internal StandardFlyByWire fbwAP = null;
        internal CruiseController ccAP = null;
        internal DirectionController dcAP = null;
        internal ProgradeThrustController speedAP = null;
        internal PitchAngularVelocityController pvc = null;
        internal YawAngularVelocityController yvc = null;

        private void findModules()
        {
            if (parent.ActiveVessel == null || parent.autopilot_module_lists.ContainsKey(parent.ActiveVessel) == false)
                return;

            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(TopModuleManager)))
                masterAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(TopModuleManager)] as TopModuleManager;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(StandardFlyByWire)))
                fbwAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(StandardFlyByWire)] as StandardFlyByWire;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(CruiseController)))
                ccAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(CruiseController)] as CruiseController;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(DirectionController)))
                dcAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(DirectionController)] as DirectionController;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(ProgradeThrustController)))
                speedAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(ProgradeThrustController)] as ProgradeThrustController;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(PitchAngularVelocityController)))
                pvc = parent.autopilot_module_lists[parent.ActiveVessel][typeof(PitchAngularVelocityController)] as PitchAngularVelocityController;
            if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(YawAngularVelocityController)))
                yvc = parent.autopilot_module_lists[parent.ActiveVessel][typeof(YawAngularVelocityController)] as YawAngularVelocityController;
        }

        public AapControler(AtmosphereAutopilot.AtmosphereAutopilot parent)
        {
            this.parent = parent;

            findModules();
        }

        public bool appButtonOn
        {
            get
            {
                return parent.launcherButtonState;
            }
        }

        public AtmosphereAutopilot.UI.Autopilots currentAutopilot
        {
            get
            {
                findModules();
                if (masterAP != null && masterAP.Active == false)
                    return AtmosphereAutopilot.UI.Autopilots.DISABLED;
                if (fbwAP != null && fbwAP.Active)
                    return AtmosphereAutopilot.UI.Autopilots.FLYBYWIRE;
                if (ccAP != null && ccAP.Active)
                    return AtmosphereAutopilot.UI.Autopilots.CRUISECTRL;
                if (dcAP != null && dcAP.Active)
                    return AtmosphereAutopilot.UI.Autopilots.MOUSEDIR;
                return AtmosphereAutopilot.UI.Autopilots.DISABLED;
            }

            set
            {
                if (masterAP == null)
                    return;

                switch (value)
                {
                    case AtmosphereAutopilot.UI.Autopilots.DISABLED:
                        fbwAP = null;
                        ccAP = null;
                        dcAP = null;
                        speedAP = null;
                        pvc = null;
                        yvc = null;
                        masterAP.Active = false;
                        parent.setLauncherOnOffIcon(false);
                        return;
                    case AtmosphereAutopilot.UI.Autopilots.FLYBYWIRE:
                        fbwAP = masterAP.activateAutopilot(typeof(StandardFlyByWire)) as StandardFlyByWire;
                        ccAP = null;
                        dcAP = null;
                        break;
                    case AtmosphereAutopilot.UI.Autopilots.CRUISECTRL:
                        fbwAP = null;
                        ccAP = masterAP.activateAutopilot(typeof(CruiseController)) as CruiseController;
                        dcAP = null;
                        break;
                    case AtmosphereAutopilot.UI.Autopilots.MOUSEDIR:
                        fbwAP = null;
                        ccAP = null;
                        dcAP = masterAP.activateAutopilot(typeof(DirectionController)) as DirectionController;
                        break;
                }
                if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(ProgradeThrustController)))
                    speedAP = parent.autopilot_module_lists[parent.ActiveVessel][typeof(ProgradeThrustController)] as ProgradeThrustController;
                if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(PitchAngularVelocityController)))
                    pvc = parent.autopilot_module_lists[parent.ActiveVessel][typeof(PitchAngularVelocityController)] as PitchAngularVelocityController;
                if (parent.autopilot_module_lists[parent.ActiveVessel].ContainsKey(typeof(YawAngularVelocityController)))
                    yvc = parent.autopilot_module_lists[parent.ActiveVessel][typeof(YawAngularVelocityController)] as YawAngularVelocityController;

                parent.setLauncherOnOffIcon(masterAP.Active);
            }
        }

        #region Speed Control

        public bool speedControl
        {
            get
            {
                return speedAP != null && speedAP.spd_control_enabled;
            }

            set
            {
                if (speedAP != null)
                    speedAP.spd_control_enabled = value;
            }
        }

        public float speed
        {
            get
            {
                if (speedAP == null)
                    return 0f;
                return speedAP.setpoint.mps();
            }

            set
            {
                if (parent.ActiveVessel != null)
                    speedAP.setpoint = new SpeedSetpoint(SpeedType.MetersPerSecond, value, parent.ActiveVessel);
            }
        }

        #endregion

        #region Fly-By-Wire

        public bool rocketMode
        {
            get
            {
                return fbwAP != null && fbwAP.rocket_mode;
            }
            set
            {
                if (fbwAP != null)
                    fbwAP.rocket_mode = value;
            }
        }

        // the "normal" value for all the limits, hardcoded
        private const float moderationNorm = 10f;

        private void setModerationLimits(float limit)
        {
            if (pvc != null)
            {
                pvc.max_aoa = limit;
                pvc.max_g_force = limit;
            }
            if (yvc != null)
            {
                yvc.max_aoa = limit;
                yvc.max_g_force = limit;
            }
        }

        public bool moderation
        {
            get
            {
                return fbwAP != null && fbwAP.moderation_switch;
            }
            set
            {
                if (fbwAP != null)
                    fbwAP.moderation_switch = value;
            }
        }

        public float moderationMult
        {
            get
            {
                if (pvc == null)
                    return 1f;
                //take pitch aoa limit as the master
                float value = pvc.max_aoa;
                setModerationLimits(value);
                return value / moderationNorm;
            }
            set
            {
                setModerationLimits(value * moderationNorm);
            }
        }

        #endregion

        #region Cruise Control

        public bool altitudeControl
        {
            get
            {
                return ccAP != null && ccAP.vertical_control;
            }
            set
            {
                if (ccAP != null)
                    ccAP.vertical_control = value;
            }
        }

        public float altitude
        {
            get
            {
                if (ccAP == null)
                    return 0f;
                return ccAP.desired_altitude;
            }
            set
            {
                if (ccAP != null)
                    ccAP.desired_altitude.Value = value;
            }
        }

        public float distToWaypoint
        {
            get
            {
                if (ccAP == null || ccAP.current_mode != CruiseController.CruiseMode.Waypoint && ccAP.waypoint_entered)
                    return -1f;

                return (float)ccAP.dist_to_dest;
            }
        }


        public bool waypointIsSet
        {
            get
            {
                return ccAP != null && ccAP.current_mode == CruiseController.CruiseMode.Waypoint && ccAP.waypoint_entered ;
            }
        }

        public void removeWaypoint()
        {
            if (ccAP == null)
                return;
            ccAP.picking_waypoint = false;
            ccAP.LevelFlightMode = true;
        }

        #endregion

    }
}
namespace AtmosphereAutopilot
{
    public sealed class DirectionController : StateController
    {
        internal DirectionController(Vessel v)
            : base(v, "Direction Director", 88437228)
        { }

        public FlightModel imodel;
        //AccelerationController acc_c;
        public DirectorController dir_c;
        public ProgradeThrustController thrust_c;

        public override void InitializeDependencies(Dictionary<Type, AutopilotModule> modules)
        {
            imodel = modules[typeof(FlightModel)] as FlightModel;
            dir_c = modules[typeof(DirectorController)] as DirectorController;
            thrust_c = modules[typeof(ProgradeThrustController)] as ProgradeThrustController;
        }

        protected override void OnActivate()
        {
            dir_c.Activate();
            thrust_c.Activate();
            MessageManager.post_status_message("Direction Director enabled");
        }

        protected override void OnDeactivate()
        {
            dir_c.Deactivate();
            thrust_c.Deactivate();
            MessageManager.post_status_message("Direction Director disabled");
        }

        public override void ApplyControl(FlightCtrlState cntrl)
        {
            if (vessel.LandedOrSplashed)
                return;


            dir_c.ApplyControl(cntrl, tgt_direction, Vector3d.zero);

            if (thrust_c.spd_control_enabled)
                thrust_c.ApplyControl(cntrl, thrust_c.setpoint.mps());
        }

        public Vector3 tgt_direction;

        public override void OnUpdate()
        {
           
        }

        [AutoGuiAttr("Director controller GUI", true)]
        protected bool DirGUI { get { return dir_c.IsShown(); } set { if (value) dir_c.ShowGUI(); else dir_c.UnShowGUI(); } }

        [AutoGuiAttr("Thrust controller GUI", true)]
        protected bool PTCGUI { get { return thrust_c.IsShown(); } set { if (value) thrust_c.ShowGUI(); else thrust_c.UnShowGUI(); } }

        protected override void _drawGUI(int id)
        {
            close_button();
            GUILayout.BeginVertical();
            AutoGUI.AutoDrawObject(this);
            thrust_c.SpeedCtrlGUIBlock();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public class CenterIndicator : MonoBehaviour
        {
            Material mat = new Material(Shader.Find("KSP/Sprite"));

            Vector3 startVector = new Vector3(0.494f, 0.5f, -0.001f);
            Vector3 endVector = new Vector3(0.506f, 0.5f, -0.001f);

            public bool enabled = false;

            public void OnPostRender()
            {
                if (enabled)
                {
                    GL.PushMatrix();
                    mat.SetPass(0);
                    mat.color = Color.red;
                    GL.LoadOrtho();
                    GL.Begin(GL.LINES);
                    GL.Color(Color.red);
                    GL.Vertex(startVector);
                    GL.Vertex(endVector);
                    GL.End();
                    GL.PopMatrix();
                    enabled = false;
                }
            }
        }


    }


}