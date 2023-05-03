using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

namespace RVP
{
    [RequireComponent(typeof(VehicleParent))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Input/Basic Input", 0)]

    // Class for setting the input with the input manager
    public class BasicInput : NetworkBehaviour // #% Mono -> Network
    {
        GameMaster master;
        InputManager input;

        VehicleParent vp;
        public string accelAxis;
        public string brakeAxis;
        public string steerAxis;
        public string ebrakeAxis;
        public string boostButton;
        public string upshiftButton;
        public string downshiftButton;
        public string pitchAxis;
        public string yawAxis;
        public string rollAxis;

        // #% My Variables
        bool localPlay = true;

        public override void OnNetworkSpawn()
        {
            localPlay = false;
        }

        void Start()
        {
            if (localPlay || SceneManager.GetActiveScene().name == Common.mainSceneName)
            {
                master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
                input = master.input;
            }

            vp = GetComponent<VehicleParent>();
        }

        void Update()
        {
            if (!localPlay)
            {
                if (!IsOwner) return;
            }

            // Get single-frame input presses

            if (!master.ready) return;

            if (!string.IsNullOrEmpty(upshiftButton))
            {
                if (Input.GetButtonDown(upshiftButton))
                    vp.PressUpshift();
            }

            if (!string.IsNullOrEmpty(downshiftButton))
            {
                if (Input.GetButtonDown(downshiftButton))
                    vp.PressDownshift();
            }
        }

        void FixedUpdate()
        {
            if (!localPlay)
            {
                if (!IsOwner) return;
            }

            // Get constant inputs

            if (!master.ready) return;

            if (!input.allowInput) return;

            if (!string.IsNullOrEmpty(accelAxis))
            {
                if (input.forceBrake)
                    vp.SetAccel(0);
                else if (input.allowDrive)
                    vp.SetAccel(Input.GetAxis(accelAxis));
            }

            if (!string.IsNullOrEmpty(brakeAxis))
            {
                if (input.forceBrake)
                {
                    vp.brakeIsReverse = false;
                    vp.SetBrake(1);
                }
                else if (input.allowDrive)
                {
                    vp.brakeIsReverse = true;
                    vp.SetBrake(Input.GetAxis(brakeAxis));
                }
            }

            if (!string.IsNullOrEmpty(steerAxis))
            {
                if (input.forceBrake)
                    vp.SetSteer(0);
                else if (input.allowDrive)
                    vp.SetSteer(Input.GetAxis(steerAxis));
            }

            if (!string.IsNullOrEmpty(ebrakeAxis))
            {
                if (input.forceBrake)
                    vp.SetEbrake(0);
                else if (input.allowDrive)
                    vp.SetEbrake(Input.GetAxis(ebrakeAxis));
            }

            if (!string.IsNullOrEmpty(boostButton))
            {
                if (input.forceBrake)
                    vp.SetBoost(false);
                else if (input.allowDrive)
                    vp.SetBoost(Input.GetButton(boostButton));
            }

            if (!string.IsNullOrEmpty(pitchAxis))
                vp.SetPitch(Input.GetAxis(pitchAxis));

            if (!string.IsNullOrEmpty(yawAxis))
                vp.SetYaw(Input.GetAxis(yawAxis));

            if (!string.IsNullOrEmpty(rollAxis))
                vp.SetRoll(Input.GetAxis(rollAxis));

            if (!string.IsNullOrEmpty(upshiftButton))
                vp.SetUpshift(Input.GetAxis(upshiftButton));

            if (!string.IsNullOrEmpty(downshiftButton))
                vp.SetDownshift(Input.GetAxis(downshiftButton));
        }
    }
}