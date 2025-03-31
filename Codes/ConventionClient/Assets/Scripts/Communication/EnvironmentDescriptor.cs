using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.communication {

    [CreateAssetMenu(fileName="Communication", menuName="ScriptableObject/CreateEnvironmentDescriptor")]
    public class EnvironmentDescriptor : ScriptableObject {

        [SerializeField] string env_name;
        [SerializeField] string server_address;
        [SerializeField] string port;

	    public string EnvName => env_name;
	    public string ServerAddress => server_address;
        public string Port => port;
    }
}
