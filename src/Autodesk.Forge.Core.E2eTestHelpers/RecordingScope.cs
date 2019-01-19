﻿/* 
 * Forge SDK
 *
 * The Forge Platform contains an expanding collection of web service components that can be used with Autodesk cloud-based products or your own technologies. Take advantage of Autodesk’s expertise in design and engineering.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Autodesk.Forge.Core.E2eTestHelpers
{
    internal class RecordingScope : TestScope
    {
        private List<JObject> records = new List<JObject>();
        private JsonSerializer serializer;

        public RecordingScope(string path)
            : base(path)
        {
            this.serializer = new JsonSerializer();
            this.serializer.Converters.Add(new HttpResponeMessageConverter());
        }

        public async override Task<HttpResponseMessage> SendAsync(HttpMessageInvoker inner, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await inner.SendAsync(request, cancellationToken);

            if (!TryRecordAuthentication(response))
            {
                var json = JObject.FromObject(response, this.serializer);
                this.records.Add(json);
            }
            return response;
        }

        public override void Dispose()
        {
            base.Dispose();
            var json = JsonConvert.SerializeObject(this.records, Formatting.Indented);
            File.WriteAllText(base.path, json);
        }

        public override bool IsRecording => true;
    }
}