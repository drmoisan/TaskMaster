#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Raised when an inbound breadcrumb bridge payload is malformed: invalid JSON, unknown
    /// <c>type</c>, or missing/wrong-typed required fields (#349). The codec logs a specific
    /// log4net error before throwing; callers must not swallow this exception silently.
    /// </summary>
    public sealed class BreadcrumbMessageException : Exception
    {
        /// <summary>Creates the exception with a diagnostic message.</summary>
        public BreadcrumbMessageException(string message)
            : base(message) { }

        /// <summary>Creates the exception wrapping the underlying JSON parse failure.</summary>
        public BreadcrumbMessageException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Serializes outbound and deserializes inbound breadcrumb bridge messages over
    /// Newtonsoft.Json (#349). Outbound JSON is camelCase-discriminated per the bridge contract;
    /// inbound parsing fails fast (log + <see cref="BreadcrumbMessageException"/>) on malformed
    /// input — no silent swallow, no broad catch without rethrow.
    /// </summary>
    public sealed class BreadcrumbMessageCodec
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static readonly JsonSerializerSettings OutboundSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        /// <summary>
        /// Serializes an outbound message to its discriminated camelCase JSON form
        /// (<c>{ "type": ..., ... }</c>; null optional fields omitted).
        /// </summary>
        /// <param name="message">The outbound message. Required.</param>
        /// <returns>The JSON payload.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        public string SerializeOutbound(BreadcrumbOutboundMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return JsonConvert.SerializeObject(message, OutboundSettings);
        }

        /// <summary>
        /// Deserializes an inbound bridge payload, validating the discriminator and the
        /// per-type required fields (<c>segmentDoubleClick</c> requires <c>segmentIndex</c>;
        /// <c>arrowKey</c> requires <c>key</c>; every type requires <c>rowId</c>).
        /// </summary>
        /// <param name="json">The raw JSON payload from the hosted document.</param>
        /// <returns>The validated inbound message.</returns>
        /// <exception cref="BreadcrumbMessageException">
        /// The payload is not valid JSON, has an unknown <c>type</c>, or is missing/wrong-typed
        /// on a required field.
        /// </exception>
        public BreadcrumbInboundMessage DeserializeInbound(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw Fail("Inbound breadcrumb payload is null or empty.");
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (JsonException ex)
            {
                log.Error($"Inbound breadcrumb payload is not valid JSON: {ex.Message}");
                throw new BreadcrumbMessageException(
                    "Inbound breadcrumb payload is not valid JSON.",
                    ex
                );
            }

            string type = RequireString(root, "type");
            if (!IsKnownInboundType(type))
            {
                throw Fail($"Unknown inbound breadcrumb message type '{type}'.");
            }

            string rowId = RequireString(root, "rowId");
            int? segmentIndex = OptionalInt(root, "segmentIndex");
            string? key = OptionalString(root, "key");

            if (type == BreadcrumbMessageTypes.SegmentDoubleClick && !segmentIndex.HasValue)
            {
                throw Fail("segmentDoubleClick requires an integer 'segmentIndex' field.");
            }

            if (type == BreadcrumbMessageTypes.ArrowKey && string.IsNullOrEmpty(key))
            {
                throw Fail("arrowKey requires a non-empty 'key' field.");
            }

            return new BreadcrumbInboundMessage(type, rowId, segmentIndex, key);
        }

        private static bool IsKnownInboundType(string type)
        {
            return type == BreadcrumbMessageTypes.SegmentDoubleClick
                || type == BreadcrumbMessageTypes.LeafExpandToggle
                || type == BreadcrumbMessageTypes.ArrowKey
                || type == BreadcrumbMessageTypes.RowSelected;
        }

        private static string RequireString(JObject root, string fieldName)
        {
            JToken? token = root[fieldName];
            if (token == null || token.Type == JTokenType.Null)
            {
                throw Fail($"Inbound breadcrumb message is missing required field '{fieldName}'.");
            }

            if (token.Type != JTokenType.String)
            {
                throw Fail(
                    $"Inbound breadcrumb field '{fieldName}' must be a string but was {token.Type}."
                );
            }

            return token.Value<string>()!;
        }

        private static int? OptionalInt(JObject root, string fieldName)
        {
            JToken? token = root[fieldName];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type != JTokenType.Integer)
            {
                throw Fail(
                    $"Inbound breadcrumb field '{fieldName}' must be an integer but was {token.Type}."
                );
            }

            return token.Value<int>();
        }

        private static string? OptionalString(JObject root, string fieldName)
        {
            JToken? token = root[fieldName];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type != JTokenType.String)
            {
                throw Fail(
                    $"Inbound breadcrumb field '{fieldName}' must be a string but was {token.Type}."
                );
            }

            return token.Value<string>();
        }

        private static BreadcrumbMessageException Fail(string message)
        {
            log.Error(message);
            return new BreadcrumbMessageException(message);
        }
    }
}
