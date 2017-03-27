using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Seeking")]
    public class SeekSteeringBehaviour : SeekFleeBehaviour {

        public override SteeringOutput GetSteering() {
            throw new NotImplementedException();
        }

    }
}
