using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace QueueTorrent
{
	public class IPEndPointJsonConverter : JsonConverter<IPEndPoint>
	{
		public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var v = reader.GetString();
			return v != null && IPEndPoint.TryParse(v, out var result) ? result : null;
		}

		public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class TorrentSettings
    {
        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint? ListenEndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 55123);

		[JsonConverter(typeof(IPEndPointJsonConverter))]
		public IPEndPoint? DhtEndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 55123);

		[Required]
        public bool AllowPortForwarding { get; set; } = true;

        [Required]
        public bool AllowLocalPeerDiscovery { get; set; } = true;

        [Required]
        public string DownloadFolder { get; set; } = "Downloads";

        [Required]
        public bool UseTorrentHotFolder { get; set; } = true;

        [Required]
        public string TorrentHotFolder { get; set; } = "HotFolder";

        [Required]
        public bool DeleteHotFolderTorrentAfterAdding { get; set; } = true;

        [Required]
        public int MaximumDownloadRate { get; set; } = 0;

        [Required]
        public int MaximumUploadRate { get; set; } = 0;

        [Required]
        public int MaximumActiveDownloads { get; set; } = 5;

        [Required]
        public int MaximumActiveUploads { get; set; } = 5;

        [Required]
        public double SeedLimit { get; set; } = 2.0;
    }
}
