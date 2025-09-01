from flask import Flask, render_template_string, request
import os, redis

app = Flask(__name__)
r = redis.Redis(host=os.getenv("REDIS_HOST","redis"), port=int(os.getenv("REDIS_PORT","6379")), db=0)

TEMPLATE = """
<html><body>
<h1>Vote</h1>
<form method="POST">
<button name="vote" value="cats">Cats</button>
<button name="vote" value="dogs">Dogs</button>
</form>
</body></html>
"""

@app.route("/", methods=["GET","POST"])
def vote():
    if request.method == "POST":
        v = request.form.get("vote")
        if v in ("cats", "dogs"):
            r.lpush("votes", v)
    return render_template_string(TEMPLATE)
