using UnityEngine;
using System.Collections;

namespace RVP
{
    [RequireComponent(typeof(VehicleParent))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Effects/Light Controller", 2)]

    // Class for controlling vehicle lights
    public class LightController : MonoBehaviour
    {
        VehicleParent vp;

        public bool headlightsOn;
        public bool highBeams;
        public bool brakelightsOn;
        public bool rightBlinkersOn;
        public bool leftBlinkersOn;
        public float blinkerInterval = 0.3f;
        bool blinkerIntervalOn;
        float blinkerSwitchTime;
        public bool reverseLightsOn;

        public Transmission transmission;
        GearboxTransmission gearTrans;
        ContinuousTransmission conTrans;

        public VehicleLight[] headlights;
        public VehicleLight[] brakeLights;
        public VehicleLight[] RightBlinkers;
        public VehicleLight[] LeftBlinkers;
        public VehicleLight[] ReverseLights;

        // #% My Variables
        public bool padControlLights = false;
        int headlightSwitch = 0;
        bool keyBracketsPressed = false;
        bool padAxis6Pressed = false;
        bool padAxis7Pressed = false;

        void Start()
        {
            vp = GetComponent<VehicleParent>();

            // Get transmission for using reverse lights
            if (transmission)
            {
                if (transmission is GearboxTransmission)
                {
                    gearTrans = transmission as GearboxTransmission;
                }
                else if (transmission is ContinuousTransmission)
                {
                    conTrans = transmission as ContinuousTransmission;
                }
            }
        }

        void Update()
        {
            #region #% Keyboard inputs to activate blinkers

            // Headlights: H
            if (Input.GetKeyDown(KeyCode.H))
                SwitchHeadlights();

            // Left blinkers: [
            if (Input.GetKeyDown(KeyCode.LeftBracket))
                ToggleLeftBlinkers();

            // Right blinkers: ]
            if (Input.GetKeyDown(KeyCode.RightBracket))
                ToggleRightBlinkers();

            // Hazard lights: [ ]
            if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightBracket))
            {
                if (!keyBracketsPressed)
                {
                    keyBracketsPressed = true;
                    ToggleHazardLights();
                }
            }
            else
                keyBracketsPressed = false;

            #endregion

            #region #% Gamepad inputs to activate blinkers

            // Toggle vehicle light controls: Gamepad LB
            if (Input.GetAxisRaw("Gamepad LB") > 0)
            {
                padControlLights = true;

                // Headlights (up button = 1)
                if (Input.GetAxisRaw("Gamepad Up/Down Buttons") > 0)
                {
                    if (!padAxis7Pressed)
                    {
                        padAxis7Pressed = true;
                        SwitchHeadlights();
                    }
                }
                // Hazard lights (down button = -1)
                else if (Input.GetAxisRaw("Gamepad Up/Down Buttons") < 0)
                {
                    if (!padAxis7Pressed)
                    {
                        padAxis7Pressed = true;
                        ToggleHazardLights();
                    }
                }
                else
                    padAxis7Pressed = false;

                // Left blinkers (left button = -1)
                if (Input.GetAxisRaw("Gamepad Left/Right Buttons") < 0)
                {
                    if (!padAxis6Pressed)
                    {
                        padAxis6Pressed = true;
                        ToggleLeftBlinkers();
                    }
                }
                // Right blinkers (right button = 1)
                else if (Input.GetAxisRaw("Gamepad Left/Right Buttons") > 0)
                {
                    if (!padAxis6Pressed)
                    {
                        padAxis6Pressed = true;
                        ToggleRightBlinkers();
                    }
                }
                else
                    padAxis6Pressed = false;
            }
            else
                padControlLights = false;

            #endregion

            #region #% Logic chunks for blinkers

            void SwitchHeadlights()
            {
                if (headlightSwitch == 2) headlightSwitch = 0;
                else headlightSwitch++;

                switch (headlightSwitch)
                {
                    case 0:
                        headlightsOn = false;
                        highBeams = false;
                        break;
                    case 1:
                        headlightsOn = true;
                        highBeams = false;
                        break;
                    case 2:
                        headlightsOn = true;
                        highBeams = true;
                        break;
                }
            }

            void ToggleLeftBlinkers()
            {
                leftBlinkersOn = !leftBlinkersOn;
                rightBlinkersOn = false;
            }

            void ToggleRightBlinkers()
            {
                rightBlinkersOn = !rightBlinkersOn;
                leftBlinkersOn = false;
            }

            void ToggleHazardLights()
            {
                if (leftBlinkersOn && rightBlinkersOn)
                {
                    leftBlinkersOn = false;
                    rightBlinkersOn = false;
                }
                else
                {
                    leftBlinkersOn = true;
                    rightBlinkersOn = true;
                }
            }

            #endregion

            // Activate blinkers
            if (leftBlinkersOn || rightBlinkersOn)
            {
                if (blinkerSwitchTime == 0)
                {
                    blinkerIntervalOn = !blinkerIntervalOn;
                    blinkerSwitchTime = blinkerInterval;
                }
                else
                {
                    blinkerSwitchTime = Mathf.Max(0, blinkerSwitchTime - Time.deltaTime);
                }
            }
            else
            {
                blinkerIntervalOn = false;
                blinkerSwitchTime = 0;
            }

            // Activate reverse lights
            if (gearTrans)
            {
                reverseLightsOn = gearTrans.curGearRatio < 0;
            }
            else if (conTrans)
            {
                reverseLightsOn = conTrans.reversing;
            }

            // Activate brake lights
            if (vp.accelAxisIsBrake)
            {
                brakelightsOn = vp.accelInput != 0 && Mathf.Sign(vp.accelInput) != Mathf.Sign(vp.localVelocity.z) && Mathf.Abs(vp.localVelocity.z) > 1;
            }
            else
            {
                if (!vp.brakeIsReverse)
                {
                    brakelightsOn = (vp.burnout > 0 && vp.brakeInput > 0) || vp.brakeInput > 0;
                }
                else
                {
                    brakelightsOn = (vp.burnout > 0 && vp.brakeInput > 0) || ((vp.brakeInput > 0 && vp.localVelocity.z > 1) || (vp.accelInput > 0 && vp.localVelocity.z < -1));
                }
            }

            SetLights(headlights, highBeams, headlightsOn);
            SetLights(brakeLights, headlightsOn || highBeams, brakelightsOn);
            SetLights(RightBlinkers, rightBlinkersOn && blinkerIntervalOn);
            SetLights(LeftBlinkers, leftBlinkersOn && blinkerIntervalOn);
            SetLights(ReverseLights, reverseLightsOn);
        }

        // Set if lights are on or off based on the condition
        void SetLights(VehicleLight[] lights, bool condition)
        {
            foreach (VehicleLight curLight in lights)
            {
                curLight.on = condition;
            }
        }

        // Set if lights are on or off based on the first condition, and half on based on the second condition (see halfOn tooltip in VehicleLight)
        void SetLights(VehicleLight[] lights, bool condition, bool halfCondition)
        {
            foreach (VehicleLight curLight in lights)
            {
                curLight.on = condition;
                curLight.halfOn = halfCondition;
            }
        }
    }
}
