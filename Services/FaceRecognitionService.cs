using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace OSVS.Services
{
	//public class FaceVerificationService
	//{
	//	private readonly IFaceClient _faceClient;
	//	private readonly ILogger<FaceVerificationService> _logger;
	//	private const double ConfidenceThreshold = 0.5;

	//	public FaceVerificationService(string endpoint, string subscriptionKey, ILogger<FaceVerificationService> logger)
	//	{
	//		_faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey))
	//		{
	//			Endpoint = endpoint
	//		};
	//		_logger = logger;
	//	}

	//	public async Task<Guid?> DetectFaceFromStreamAsync(Stream imageStream)
	//	{
	//		try
	//		{
	//			var detectedFaces = await _faceClient.Face.DetectWithStreamAsync(
	//				imageStream,
	//				returnFaceId: true,
	//				recognitionModel: RecognitionModel.Recognition04,
	//				detectionModel: DetectionModel.Detection03);

	//			return detectedFaces.FirstOrDefault()?.FaceId;
	//		}
	//		catch (APIErrorException apiEx)
	//		{
	//			_logger.LogError($"Face API Error: {apiEx.Body?.Error?.Message ?? apiEx.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			_logger.LogError($"Unexpected Error: {ex.Message}");
	//			throw;
	//		}
	//	}


	//	public async Task<Guid?> DetectFaceFromPathAsync(string imagePath)
	//	{
	//		if (string.IsNullOrWhiteSpace(imagePath))
	//		{
	//			throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));
	//		}

	//		try
	//		{
	//			using (var imageStream = File.OpenRead(imagePath))
	//			{
	//				return await DetectFaceFromStreamAsync(imageStream);
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			_logger.LogError($"Error detecting face from path: {ex.Message}");
	//			throw;
	//		}
	//	}

	//	public async Task<bool> VerifyFaceAsync(string storedImagePath, Stream capturedImageStream)
	//	{
	//		try
	//		{
	//			// Detect face from stored image
	//			var storedFaceId = await DetectFaceFromPathAsync(storedImagePath);
	//			if (storedFaceId == null)
	//			{
	//				_logger.LogWarning("No face detected in stored image.");
	//				return false;
	//			}

	//			// Detect face from captured image
	//			var capturedFaceId = await DetectFaceFromStreamAsync(capturedImageStream);
	//			if (capturedFaceId == null)
	//			{
	//				_logger.LogWarning("No face detected in captured image.");
	//				return false;
	//			}

	//			// Verify the faces
	//			var verifyResult = await _faceClient.Face.VerifyFaceToFaceAsync(storedFaceId.Value, capturedFaceId.Value);
	//			_logger.LogInformation($"Verification result: IsIdentical={verifyResult.IsIdentical}, Confidence={verifyResult.Confidence}");

	//			return verifyResult.IsIdentical && verifyResult.Confidence > ConfidenceThreshold;
	//		}
	//		catch (APIErrorException ex)
	//		{
	//			_logger.LogError($"Face API Error: {ex.Body?.Error?.Message ?? ex.Message}");
	//			return false;
	//		}
	//		catch (Exception ex)
	//		{
	//			_logger.LogError($"Unexpected error during face verification: {ex.Message}");
	//			return false;
	//		}
	//	}

	//}

	public class FaceVerificationService
	{
		private readonly string _scriptPath;
		private readonly ILogger<FaceVerificationService> _logger;

		public FaceVerificationService(string scriptPath, ILogger<FaceVerificationService> logger)
		{
			_scriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public (bool, double) VerifyFaces(string storedImagePath, string capturedImageBase64)
		{
			string tempCapturedImagePath = Path.GetTempFileName();

			try
			{
				// Convert Base64 string to binary and save to temporary file
				var imageBytes = Convert.FromBase64String(capturedImageBase64.Replace("data:image/jpeg;base64,", ""));
				File.WriteAllBytes(tempCapturedImagePath, imageBytes);

				// Call the Python script
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "python",
						Arguments = $"\"{_scriptPath}\" \"{storedImagePath}\" \"{tempCapturedImagePath}\"",
						RedirectStandardOutput = true,  // Capture stdout
						RedirectStandardError = true,  // Capture stderr for debugging
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				process.Start();

				// Read stdout and stderr
				string standardOutput = process.StandardOutput.ReadToEnd().Trim();
				string standardError = process.StandardError.ReadToEnd().Trim();

				// Log stderr for debugging
				if (!string.IsNullOrWhiteSpace(standardError))
				{
					_logger.LogWarning($"Python Script Debug (stderr): {standardError}");
				}

				_logger.LogInformation($"Python Script Result (stdout): {standardOutput}");

				process.WaitForExit();

				// Parse the result from stdout
				var parts = standardOutput.Split(',');
				if (parts.Length != 2)
				{
					throw new FormatException($"Invalid output format from Python script. Output: [{standardOutput}]");
				}

				if (!bool.TryParse(parts[0], out bool isMatch))
				{
					throw new FormatException($"Invalid boolean value: {parts[0]}");
				}

				if (!double.TryParse(parts[1], out double confidence))
				{
					throw new FormatException($"Invalid confidence value: {parts[1]}");
				}

				return (isMatch, confidence);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in face verification: {ex.Message}");
				return (false, 0.0);
			}
			finally
			{
				if (File.Exists(tempCapturedImagePath))
				{
					File.Delete(tempCapturedImagePath);
				}
			}
		}




	}


}
