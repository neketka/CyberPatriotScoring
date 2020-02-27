import hashlib


def check(args):
    with open(args["path"], "r") as file:
        m = hashlib.sha256()
        m.update(file.read())
        hash = m.hexdigest()
        return hash == args["hash"]