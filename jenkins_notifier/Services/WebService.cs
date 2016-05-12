using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using jenkins_notifier.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace jenkins_notifier.Services
{
	public class WebService
	{
		public int Timeout { get; set; }
		public string Url { get; set; }

		public WebService ()
		{
		}

		private static readonly Encoding encoding = Encoding.UTF8;

		public JsonPayload<T> Get<T>(string suffixUrl) {
			JsonPayload<T> result = new JsonPayload<T>();
			var request = (HttpWebRequest)WebRequest.Create(Url + suffixUrl);
			request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			request.Method = "GET";
			if (Timeout > 0) {
				request.Timeout = Timeout;
			}

			try {
				var response = request.GetResponse();
				var respStream = response.GetResponseStream();
				respStream.Flush();

				using (StreamReader sr = new StreamReader(respStream)) {
					string strContent = sr.ReadToEnd();
					respStream = null;

					result.Payload = JsonConvert.DeserializeObject<T>(strContent);
					return result;
				}
			} catch (Exception ex) {
				result.Errored = true;
				result.Exception = ex.Message;
			}
			return result;
		}

		/// <summary>
		/// Makes an asynchronous GET request.
		/// </summary>
		/// <returns>The string response from the server</returns>
		/// <param name="path">Path which we are making our request to.</param>
		public async Task<JsonPayload<T>> GetAsync<T>(string suffixUrl) {
			JsonPayload<T> result = new JsonPayload<T>();
			var request = (HttpWebRequest)WebRequest.Create(Url + suffixUrl);
			request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			request.Method = "GET";
			if (Timeout > 0) {
				request.Timeout = Timeout;
			}

			try {
				var response = await request.GetResponseAsync();
				var respStream = response.GetResponseStream();
				respStream.Flush();

				using (StreamReader sr = new StreamReader(respStream)) {
					string strContent = sr.ReadToEnd();
					respStream = null;

					result.Payload = JsonConvert.DeserializeObject<T>(strContent);
					return result;
				}
			} catch (Exception ex) {
				result.Errored = true;
				result.Exception = ex.Message;
			}
			return result;
		}

		/// <summary>
		/// Makes an asynchronous POST request.
		/// </summary>
		/// <returns>The string response from the server</returns>
		/// <param name="path">Path which we are making our request to</param>
		/// <param name="data">POST data</param>
		/// <param name="isJson">Whether or not our ContentType is JSON</param>
		public async Task<JsonPayload<T>> PostAsync<T>(string path, object data, bool isJson = true) {
			JsonPayload<T> result = new JsonPayload<T>();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + path);
			string postData = string.Empty;

			if (isJson) {
				request.ContentType = "application/json; charset=UTF-8";
				postData = JsonConvert.SerializeObject(data);
			} else {
				request.ContentType = "application/x-www-form-urlencoded";
			}
			request.Method = "POST";
			request.Accept = "application/json";

			try {
				var stream = await request.GetRequestStreamAsync();
				using (var writer = new StreamWriter(stream)) {
					writer.Write(postData);
				}

				var response = await request.GetResponseAsync();
				var respStream = response.GetResponseStream();

				using (StreamReader sr = new StreamReader(respStream)) {
					string content = sr.ReadToEnd();
					result = JsonConvert.DeserializeObject<JsonPayload<T>>(content);
					return result;
				}
			} catch (Exception ex) {
				result.Errored = true;
				result.Exception = ex.Message;
			}
			return result;
		}

		/// <summary>
		/// Makes an asynchronous multipart POST request.
		/// </summary>
		/// <returns>The string reponse from the server</returns>
		/// <param name="path">Path which we are making our request to</param>
		/// <param name="postParameters">Post parameters.</param>
		public async Task<string> PostMultipartAsync(string path, Dictionary<string, object> postParameters) {
			string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
			string contentType = "multipart/form-data; boundary=" + formDataBoundary;

			byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

			return await PostForm(Url + path, contentType, formData);
		}

		/// <summary>
		/// Posts the multipart form.
		/// </summary>
		/// <returns>The string response from the server</returns>
		/// <param name="postUrl">Post URL</param>
		/// <param name="contentType">Content type</param>
		/// <param name="formData">Form data</param>
		private async static Task<string> PostForm(string postUrl, string contentType, byte[] formData) {
			HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

			if (request == null) {
				throw new NullReferenceException("Request is not an HTTP request");
			}

			request.Method = "POST";
			request.ContentType = contentType;

			// Send the form data to the request.
			using (Stream requestStream = await request.GetRequestStreamAsync()) {
				requestStream.Write(formData, 0, formData.Length);

			}

			WebResponse resp = await request.GetResponseAsync();
			using (Stream stream = resp.GetResponseStream()) {
				StreamReader respReader = new StreamReader(stream);
				return respReader.ReadToEnd();
			}

		}

		/// <summary>
		/// Creates the multipart form.
		/// </summary>
		/// <returns>The multipart form data</returns>
		/// <param name="postParameters">Post parameters</param>
		/// <param name="boundary">Boundary</param>
		private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary) {
			using (Stream formDataStream = new System.IO.MemoryStream()) {
				bool needsCLRF = false;

				foreach (var param in postParameters) {
					// Add a CRLF to allow multiple parameters to be added.
					// Skip it on the first parameter, add it to subsequent parameters.
					if (needsCLRF)
						formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

					needsCLRF = true;

					if (param.Value is FileParameter) {
						FileParameter fileToUpload = (FileParameter)param.Value;

						// Add just the first part of this param, since we will write the file data directly to the Stream
						string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
							boundary,
							param.Key,
							fileToUpload.FileName ?? param.Key,
							fileToUpload.ContentType ?? "application/octet-stream");

						formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

						// Write the file data directly to the Stream, rather than serializing it to a string.
						formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
					} else {
						string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
							boundary,
							param.Key,
							param.Value);
						formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
					}
				}

				// Add the end of the request.  Start with a newline
				string footer = "\r\n--" + boundary + "--\r\n";
				formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

				// Dump the Stream into a byte[]
				formDataStream.Position = 0;
				byte[] formData = new byte[formDataStream.Length];
				formDataStream.Read(formData, 0, formData.Length);

				return formData;
			}
		}

		public static async Task<byte[]> DownloadFile(string url) {
			HttpClient client = new System.Net.Http.HttpClient();
			return await client.GetByteArrayAsync(url);
		}


		/// <summary>
		/// When using multipart forms, we'll wrap our files in this class.
		/// </summary>
		public class FileParameter {
			public byte[] File { get; set; }
			public string FileName { get; set; }
			public string ContentType { get; set; }
			public FileParameter(byte[] file) : this(file, null) { }
			public FileParameter(byte[] file, string filename) : this(file, filename, null) { }

			public FileParameter(byte[] file, string fileName, string contentType) {
				File = file;
				FileName = fileName;
				ContentType = contentType;
			}
		}




	}
}

