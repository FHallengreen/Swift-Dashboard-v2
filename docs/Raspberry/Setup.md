## Setup Instructions for Raspberry Pi

- Installed Raspberry Image on SD Card via Raspberry Pi Imager. Including WiFi and Ssh enabled.
- First booted up Raspberry Pi and connected via SSH.
- Activated VNC Server on Raspberry Pi for remote desktop access.
- Updated and upgraded PI and installed git:
  ```bash
  sudo apt update && sudo apt upgrade -y
  sudo apt install git -y
  ```

- Cloned the Swift Dashboard repository (after adding SSH key to GitHub):
  ```bash
    git clone git@github.com:FHallengreen/Swift-Dashboard-v2.git
    ```

- Installed Docker:
    ```bash
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    ```

- Installed Github Runner & configured it to run as a service. And made it run as a service on the Raspberry PI so it always is online to pick up jobs.
- Configured a workflow to deploy on self-hosted runner on the Raspberry Pi.

- Installed Cloudflared to expose the local webserver to the internet for easy access to the dashboard from anywhere.

- Added CNAME record in Cloudflare to point to the Cloudflared URL for easy access.
- Pushed Docker-Compose file to the repository to make deployment easy.