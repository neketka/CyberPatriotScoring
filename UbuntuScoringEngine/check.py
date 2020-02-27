from types import ModuleType


class ImportedCheck:
    def __init__(self, name, code):
        self.name = name
        self.compiled = compile(code, name, "exec")
        self.module = ModuleType(name)
        exec(self.compiled, self.module.__dict__)

    def check(self, args):
        return self.module.check(args)


class ExtendedCheck:
    def __init__(self, name, params, importedCheck, importedArgs):
        self.name = name
        self.params = params
        self.importedCheck = importedCheck
        self.importedArgs = importedArgs

    def check(self, args):
        newArgs = dict(self.importedArgs)
        for k, v in self.importedArgs.items():
            for param in self.params:
                val = args[param]
                newArgs[k] = newArgs[k].replace("$" + param, val)
        return self.importedCheck.check(newArgs)


class StatefulPackedCheck:
    def __init__(self, checks, checksArgs, pointValue: int, text: str, conditionString: str):
        self.checks = checks
        self.checksArgs = checksArgs
        self.pointValue = pointValue
        self.state = False
        self.gained = False
        self.lost = False
        self.text = text
        self.conditionString = conditionString.strip().split(" ")

    def setStateDefault(self, state):
        self.state = state
        self.gained = False
        self.lost = False

    def setState(self, state):
        if state == self.state:
            self.gained = False
            self.lost = False
        elif state:
            self.gained = True
            self.lost = False
        else:
            self.gained = False
            self.lost = True
        self.state = state

    def getText(self):
        return self.text

    def getPoints(self):
        return self.pointValue

    def getState(self):
        return self.state

    def isGain(self):
        return self.gained

    def isLoss(self):
        return self.lost

    def evalConditions(self, states):
        stack = []
        for token in self.conditionString:
            if token == "&":
                last = stack.pop()
                first = stack.pop()
                stack.append(first and last)
            elif token == "|":
                last = stack.pop()
                first = stack.pop()
                stack.append(first or last)
            elif token == "!":
                last = stack.pop()
                stack.append(not last)
            elif token.isnumeric():
                stack.append(states[int(token)])
        return stack[0]

    def doCurrentCheck(self):
        states = []
        for i in range(0, len(self.checks)):
            states.append(self.checks[i].check(self.checksArgs[i]))
        self.setState(self.evalConditions(states))
