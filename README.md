# VisionAI - Real-Time Image Recognition with MJPEG and Telegram Integration

## Overview

VisionAI leverages cutting-edge technology to offer real-time image recognition capabilities through MJPEG streams. It integrates seamlessly with Telegram to deliver instantaneous notifications and images when specific objects are detected, focusing heavily on person detection.

## Features

- **Real-Time Image Recognition**: Harnesses the YOLOv5 model to detect objects within MJPEG streams.
- **Telegram Bot Integration**: Automatically sends alerts and images via Telegram based on detection criteria.
- **Customizable Object Detection**: Configurable settings allow for tailored object recognition.
- **Efficient Image Processing**: Optimized for balancing between speed and accuracy of detections.

## Requirements

- **Software**:
  - .NET Framework
  - FFmpeg
  - MJPEG Server
  - YOLOv5 Model
  - Telegram Bot API

- **Libraries**:
  - Newtonsoft.Json
  - MjpegProcessor
  - Telegram.Bot

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repository/VisionAI.git
   cd VisionAI

## Install Dependencies

- Ensure the .NET Framework and necessary libraries are installed.

## Set Up Telegram Bot

- Create a bot on Telegram and secure an API token.
- Replace the placeholder token in the code with your actual bot token.

## Configure MJPEG Stream

- Configure your camera or another video source to stream in MJPEG format.
- Update the camera settings in the `StartMJPEGserverProcess` method accordingly.

## Usage

1. **Start the Application**:
   - Run the application from Visual Studio.
   - Click "START" to begin streaming via MJPEG.

2. **Receive Notifications**:
   - Interact with the Telegram bot to start receiving detection alerts.
   - Notifications include images when specified objects, like persons, are detected.

3. **Monitor Image Processing**:
   - Processed images are displayed in a PictureBox with highlighted and labeled detected objects.

## Code Structure

- **Form1.cs**: Contains the main logic for handling MJPEG streams, image processing, and Telegram interactions.
- **Program.cs**: Entry point of the application.
- **Utils**: Includes utility methods for image processing and data management.

## Configuration

- Adjust detection parameters as needed in the `showSmartImage` method.
- Customize the notifications sent via Telegram in the `OnMessage` and `ErrorMessage` methods.

## Contributing

- Fork the repository and create a new branch for your feature or bug fix.
- Submit pull requests with a comprehensive description of changes.


