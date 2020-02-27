#!/usr/bin/python3

from engine import ScoringEngine
from imageLoader import ImageFile
from os import path
import time

def main():
    if not path.isfile("/scoring/imagefile.dat"):
        print("Image file not found!")
        return

    image = ImageFile("/scoring/imagefile.dat")

    engine = ScoringEngine()
    engine.setGainSoundPath("/scoring/gain.wav")
    engine.setLossSoundPath("/scoring/alarm.wav")
    engine.setNotificationIconPath("/scoring/icon.png")
    engine.setReadmePath("/scoring/readme.html")
    engine.setReportPath("/scoring/report.html")
    engine.setLockPath("/scoring/lock")
    engine.setImage(image)

    engine.initializeLock()

    try:
        while True:
            engine.update()
            time.sleep(engine.getTimeout())
    finally:
        engine.close()
        print("Scoring Engine stopped.")


if __name__ == "__main__":
    main()