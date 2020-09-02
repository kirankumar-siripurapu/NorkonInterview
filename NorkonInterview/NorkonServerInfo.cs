using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorkonInterview
{
    class NorkonServerInfo
    {
        [JsonProperty("serverName")]
        public string ServerName { get; set; }

        [JsonProperty("quantHubs")]
        public QuantHubs QuantHubs { get; set; }

        [JsonProperty("process")]
        public Process Process { get; set; }

        [JsonProperty("updates")]
        public Updates Updates { get; set; }

        [JsonProperty("reqCount")]
        public ReqCount HttpRecCount { get; set; }

        [JsonProperty("fallbackMgr")]
        public FallBackMgr FallbackMgr { get; set; }
    }

    class Process
    {
        [JsonProperty("uptime")]
        public int Uptime { get; set; }
    }

    class Updates
    {
        [JsonProperty("frag")]
        public int Frag { get; set; }
    }
    class FallBackMgr
    {
        [JsonProperty("fallbackReady")]
        public bool FallbackReady { get; set; }
    }

    class ReqCount
    {
        [JsonProperty("http")]
        public int Http { get; set; }
    }

    class QuantHubs
    {
        [JsonProperty("connected")]
        public int Connected { get; set; }
    }


}

