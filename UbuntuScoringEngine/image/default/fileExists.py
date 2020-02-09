import os.path


def check(args):
    return os.path.isfile(args["path"])