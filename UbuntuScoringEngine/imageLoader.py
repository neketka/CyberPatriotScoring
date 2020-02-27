import check
import json


class ImageFile:
    def __init__(self, path):
        with open(path, "r") as f:
            self.document = json.load(f)
        self.imports = self.document["pipModules"]
        self.scoringServer = self.document["scoringServer"]
        self.platform = self.document["platform"]
        self.pollingRate = self.document["pollingRate"]
        self.readme = self.document["readme"]
        self.timestamp = self.document["timestamp"]
        self.totalPoints = self.document["totalPoints"]
        self.data = self.document["imgData"]
        self.checks = {}
        self.vulns = []
        self.pens = []

    def getDataDict(self):
        return self.data

    def getAllImports(self):
        return self.imports

    def getAllVulns(self):
        return self.vulns

    def getAllPenalties(self):
        return self.pens

    def getScoringServer(self):
        return self.scoringServer
    
    def getPlatform(self):
        return self.platform

    def getPollingRate(self):
        return self.pollingRate

    def getTimestamp(self):
        return self.timestamp

    def getTotalPoints(self):
        return self.totalPoints

    def getReadmeHTML(self):
        return self.readme

    def loadChecks(self):
        for name, obj in self.document["checks"].items():
            self.checks[name] = check.ImportedCheck(name, obj["src"])
        for name, obj in self.document["extChecks"].items():
            self.checks[name] = check.ExtendedCheck(name, obj["params"], self.checks[obj["extends"]], obj["args"])

    def __convertToStateful(self, obj):
        checks = []
        checkArgs = []
        refs = obj["refs"]

        for chec in obj["checks"]:
            actualCheck = self.checks[chec["name"]]
            replacedArgs = dict(chec["args"])
            for k, v in chec["args"].items():
                for ref in refs:
                    if ref in self.data:
                        replacedArgs[k] = replacedArgs[k].replace("$" + ref, self.data[ref])
            checks.append(actualCheck)
            checkArgs.append(replacedArgs)

        return check.StatefulPackedCheck(checks, checkArgs, obj["points"], obj["text"], obj["condition"])

    def loadImageScoring(self):
        for obj in self.document["imgVulns"]:
            self.vulns.append(self.__convertToStateful(obj))
        for obj in self.document["imgPens"]:
            self.pens.append(self.__convertToStateful(obj))