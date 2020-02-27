from engine import ScoringEngine
from imageLoader import ImageFile
from os import path
import time
import traceback

import signal
import sys

engineClose = None

def cleanup(*args):
    engineClose()
    sys.exit(0)

signal.signal(signal.SIGINT, cleanup)
signal.signal(signal.SIGTERM, cleanup)

def main():
    if not path.isfile(r"C:\scoring\imagefile.dat"):
        print("Image file not found!")
        return

    image = ImageFile(r"C:\scoring\imagefile.dat")

    engine = ScoringEngine()
    engine.setGainSoundPath(r"C:\scoring\gain.wav")
    engine.setLossSoundPath(r"C:\scoring\alarm.wav")
    engine.setNotificationIconPath(r"C:\scoring\icon.ico")
    engine.setReadmePath(r"C:\scoring\readme.html")
    engine.setReportPath(r"C:\scoring\report.html")
    engine.setLockPath(r"C:\scoring\lock")
    engine.setImage(image)

    engine.initializeLock()

    engineClose = engine.close

    try:
        while True:
            engine.update()
            time.sleep(engine.getTimeout())
    except:
        with open("log.txt", "a") as log:
            traceback.print_exc(file=log)
    finally:
        engine.close()
        print("Scoring Engine stopped.")


if __name__ == "__main__":
    main()