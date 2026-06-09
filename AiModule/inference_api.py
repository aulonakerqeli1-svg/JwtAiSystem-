# -*- coding: utf-8 -*-
from fastapi import FastAPI
from pydantic import BaseModel
import numpy as np
import joblib
import uvicorn

app = FastAPI(title="AI Anomaly Detection API")

try:
    model    = joblib.load("model.pkl")
    features = joblib.load("features.pkl")
    print("Model u ngarkua me sukses")
except:
    model    = None
    features = []
    print("Model nuk u gjet - ekzekuto train.py")

class LoginFeatures(BaseModel):
    username:  str
    ipAddress: str
    hourOfDay: int
    dayOfWeek: int
    isSuccess: bool
    userAgent: str
    country:   str   = "Unknown"
    latitude:  float = 0
    longitude: float = 0

def ip_octet1(ip: str) -> int:
    try:    return int(ip.split(".")[0])
    except: return 0

def ua_hash(ua: str) -> int:
    if "Chrome"  in ua: return 1
    if "Firefox" in ua: return 2
    if "Safari"  in ua: return 3
    if "curl"    in ua: return 99
    return 4

@app.post("/score")
async def score(f: LoginFeatures):
    if model is None:
        return {
            "score":     0.0,
            "isAnomaly": False,
            "reason":    "Model nuk eshte trajnuar"
        }

    x = np.array([[
        f.hourOfDay,
        f.dayOfWeek,
        1 if f.isSuccess else 0,
        ip_octet1(f.ipAddress),
        0,
        ua_hash(f.userAgent)
    ]])

    raw        = model.decision_function(x)[0]
    prediction = model.predict(x)[0]
    score_val  = max(0.0, min(1.0, 0.5 - raw))

    is_anomaly = False
    reason     = "Login normal"

    if f.hourOfDay < 6:
        score_val += 0.3
        reason     = "Login ne ore te pazakonte"

    if f.country == "Brazil":
        score_val += 0.5
        reason     = "Impossible travel - Brazil"

    if ua_hash(f.userAgent) == 99:
        score_val += 0.4
        reason     = "User-agent i dyshimte curl"

    if not f.isSuccess:
        score_val += 0.1

    score_val = min(1.0, score_val)

    if score_val > 0.5 or prediction == -1:
        is_anomaly = True
        if reason == "Login normal":
            reason = "Isolation Forest anomaly"

    return {
        "score":     round(score_val, 4),
        "isAnomaly": is_anomaly,
        "reason":    reason
    }

@app.get("/health")
async def health():
    return {
        "status":       "ok",
        "model_loaded": model is not None
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
