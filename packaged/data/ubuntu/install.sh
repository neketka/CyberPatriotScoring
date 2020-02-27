#!/bin/sh

if [ ! -f "scoring.service" ]; then
   echo "Please 'cd' into this extracted directory."
   exit
fi

if [ "$EUID" -ne 0 ]
  then echo "Please run with 'sudo bash ./install.sh'."
  exit
fi

apt install curl
apt install python3

mkdir /scoring
cp ./scoring/* /scoring
cp ./scoring.service /etc/systemd/system

cp "./Scoring Report.desktop" ~/Desktop
cp "./Readme.desktop" ~/Desktop

sed -i "s/XUSER/${SUDO_USER}/g" /etc/systemd/system/scoring.service

chmod 644 /etc/systemd/system/scoring.service
chmod +x /scoring/main.py

chmod +x "${HOME}/Desktop/Scoring Report.desktop"
chmod +x "${HOME}/Desktop/Readme.desktop"

curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py
python3 get-pip.py

sudo -H pip3 install py-notifier
sudo -H pip3 install playsound

systemctl enable scoring

read -p "Press enter to reboot now..."

reboot