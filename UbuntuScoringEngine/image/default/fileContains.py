def check(args):
    f = open(args["path"], "r")
    return args["query"] in f.read()