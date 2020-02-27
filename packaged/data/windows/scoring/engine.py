from pynotifier import Notification
import playsound
import reportGen
import subprocess
import os


class ScoringEngine:
    def __init__(self):
        self.image = None
        self.gainPath = ""
        self.lossPath = ""
        self.notifIconPath = ""
        self.reportPath = ""
        self.readmePath = ""
        self.lockPath = ""
        self.closed = False
        self.lockInited = False

    def setImage(self, image):
        self.image = image

    def setGainSoundPath(self, path):
        self.gainPath = path

    def setLossSoundPath(self, path):
        self.lossPath = path

    def setNotificationIconPath(self, path):
        self.notifIconPath = path

    def setReportPath(self, path):
        self.reportPath = path
        
    def setReadmePath(self, path):
        self.readmePath = path

    def setLockPath(self, path):
        self.lockPath = path

    def getTimeout(self):
        return self.image.getPollingRate()

    def __gain(self):
        Notification(title="CCS Service",
	        description="You gained points!",
	        icon_path=self.notifIconPath,
	        duration=5,                            
	        urgency=Notification.URGENCY_NORMAL).send()
        playsound.playsound(self.gainPath)

    def __loss(self):
        Notification(title="CCS Service",
	        description="You lost points!",
	        icon_path=self.notifIconPath,
	        duration=5,                            
	        urgency=Notification.URGENCY_NORMAL).send()
        playsound.playsound(self.lossPath)

    def __writeReport(self, notif=""):
        texts = []
        pTexts = []
        points = 0
        lossPoints = 0
        maxPoints = self.image.getTotalPoints()
        maxVulns = len(self.image.getAllVulns())

        for vuln in self.image.getAllVulns():
            if vuln.getState():
                texts.append(vuln.getText())
                points += vuln.getPoints()

        for pen in self.image.getAllPenalties():
            if pen.getState():
                pTexts.append(pen.getText())
                lossPoints += pen.getPoints()

        reportGen.generateReport(self.reportPath, texts, pTexts, points, maxPoints, maxVulns, lossPoints, self.getTimeout(), notif)

    def __installDeps(self):
        pips = self.image.getAllImports()
        for pip in pips:
            subprocess.check_output(["pip", "install", pip])

    def initializeLock(self):
        existed = False
        self.image.loadChecks()
        self.image.loadImageScoring()
        vulns = self.image.getAllVulns()
        pens = self.image.getAllPenalties()

        if os.path.isfile(self.lockPath + ".lock"):
            print("Lock is not free! If engine is not running, delete '" + self.lockPath + ".lock'.")
            #raise BlockingIOError()
        else:
            open(self.lockPath + ".lock", "w+").close()

        self.lockInited = True

        if os.path.isfile(self.lockPath):
            existed = True
            states = []
            with open(self.lockPath, "r") as lock:
                data = str(lock.readline())
                for c in data:
                    if c == "0":
                        states.append(False)
                    elif c == "1":
                        states.append(True)
            for index in range(0, len(states)):
                if index >= len(vulns):
                    pens[index - len(vulns)].setStateDefault(states[index])
                else:
                    vulns[index].setStateDefault(states[index])
        else:
            with open(self.lockPath, "w") as lock:
                lock.write("0" * (len(vulns) + len(pens)))
                lock.flush()
            with open(self.readmePath, "w") as readme:
                readme.write(self.image.getReadmeHTML())
                readme.flush()
            self.__installDeps()
        return existed

    def __saveLock(self):
        stateString = ""
        for vuln in self.image.getAllVulns():
            stateString += "1" if vuln.getState() else "0"
        for pen in self.image.getAllPenalties():
            stateString += "1" if pen.getState() else "0"
        with open(self.lockPath, "w") as lock:
            lock.write(stateString)
            lock.flush()

    def close(self):
        if self.closed or not self.lockInited:
            return
        self.__writeReport("Scoring Engine is not running!")
        self.__saveLock()
        os.remove(self.lockPath + ".lock")
        self.closed = True

    def update(self):
        doGainSound = False
        doLossSound = False
        for vuln in self.image.getAllVulns():
            try:
                vuln.doCurrentCheck()
                if vuln.isGain():
                    doGainSound = True
                elif vuln.isLoss():
                    doLossSound = True
            except:
                vuln.setStateDefault(False)

        for pen in self.image.getAllPenalties():
            try:
                pen.doCurrentCheck()
                if pen.isGain():
                    doLossSound = True
                elif pen.isLoss():
                    doGainSound = True
            except:
                pen.setStateDefault(False)
        self.__writeReport("")
        self.__saveLock()
        if doGainSound:
            self.__gain()
        if doLossSound:
            self.__loss()