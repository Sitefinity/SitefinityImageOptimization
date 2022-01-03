# Sitefinity Image Optimization
This repository contains an example project that uses third party services like [Kraken IO](https://kraken.io/) and [Tinify](https://tinypng.com/) to automatically optimize images in Sitefinity CMS.

## How to setup

1. Build the solution.
2. Setup an IIS website pointing to the SitefinityWebApp and proceed with the standard Sitefinity setup.
3. Refer to the details below and enable and configure the image optimization

## Configurations
### Kraken IO
*The configurations for Kraken IO are located in **Administration > Settings > Advanced > Libraries > FileProcessors > Kraken IO Image Optimization***
 - **Enabled (boolean)**: Configuration for enabling/disabling the file processor.
 - **Name (string)**: The name of the file processor.
 - **Description (string)**: The description of the file processor.
 - **Type (string)**: The type of the file processor.
 - **Parameters**
	 - **SupportedExtensions (string)**: A comma separated list of the supported extensions. 
		 - Default value: ".jpg;.jpeg;.png"
	 - **ApiKey (string)**: The API key provided by Kraken IO.
		 - Default value: empty
	 - **ApiSecret (string)**: The API secret provided by Kraken IO.
		 - Default value: empty
	 - **LossyCompression (boolean)**: Configuration for enabling/disabling lossy compression. When enabled, the lossy optimization will give better image optimization results with just a fraction of image quality loss (usually unnoticeable to the human eye). 
		 - Default value: false
	 - **PreserveMetadata (boolean)**: Configuration for enabling/disabling meta data preserving. By default, when an image is optimized all the metadata found in an image will be stripped to make the image file as small as it is possible, in both lossy and lossless modes. Entries like EXIF, XMP and IPTC tags, colour profile information, etc. will be stripped. When enabled, the metada information is preserved. 
		 - Default value: false
	 - **TimeoutAfter (number)**: The duration (in seconds) after which the image optimization will time out. When it times out, the original (non-optimized) image is uploaded as a fallback.
		 - Default value: 30
	 - **WebpCompression**: Configuration for enabling/disabling WebP compression. When enabled, the PNG and JPEG images will be recompressed and persisted into WebP format.
		 - Default value: false
### Tinify
*The configurations for Tinify are located in **Administration > Settings > Advanced > Libraries > FileProcessors > Tinify Image Optimization***
 - **Enabled (boolean)**: Configuration for enabling/disabling the file processor.
 - **Name (string)**: The name of the file processor.
 - **Description (string)**: The description of the file processor.
 - **Type (string)**: The type of the file processor.
 - **Parameters**
	 - **SupportedExtensions (string)**: A comma separated list of the supported extensions. 
		 - Default value: ".jpg;.jpeg;.png"
	 - **ApiKey (string)**: The API key provided by Tinify.
		 - Default value: empty
	 - **PreserveMetadata (boolean)**: Configuration for enabling/disabling meta data preserving. By default, when an image is optimized all the metadata found in an image will be stripped to make the image file as small as it is possible, in both lossy and lossless modes. Entries like EXIF, XMP and IPTC tags, colour profile information, etc. will be stripped. When enabled, the metada information is preserved. 
		 - Default value: false
	 - **TimeoutAfter (number)**: The duration (in seconds) after which the image optimization will time out. When it times out, the original (non-optimized) image is uploaded as a fallback.
		 - Default value: 30
### Background Image Optimization
*The configurations for the background image optimization of existing non-optimized images are located in **Administration > Settings > Advanced > ImageOptimization***
 - **Enable image optimization scheduled task (boolean)**: Configuration for enabling/disabling the image optimization scheduled task. When enabled the image optimization scheduled task will be scheduled and executed.
	  - Default value: false
 - **Image optimization cron specification (string)**: A configuration that specifies commands to run periodically on a given schedule. For example: 5 * * * * (run on the fifth minute of every hour)
	  - Default value: 0 * * * * (every hour)
 - **Batch size (number)**: The number of images that will be processed on each scheduled task execution.
	  - Default value: 100
 - **Enable image optimization detail logging (boolean)**: Configuration for enabling/disabling detailed logging for background image optimization. When enabled the image optimization scheduled task will log detailed information on the processed images.
	  - Default value: false
 
### Global
A global configuration for disabling the image optimization feature. The configuration will take priority over the other configurations for enabling a file processor and background image optimization.
To configure the global configuration add an app setting in the **web.config** called **sf:disableImageOptimization**:

    <add key="sf:disableImageOptimization" value="True" />

