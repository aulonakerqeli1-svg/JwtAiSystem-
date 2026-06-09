# -*- coding: utf-8 -*-
import numpy as np
import pandas as pd
from sklearn.ensemble import IsolationForest
import joblib

np.random.seed(42)
n = 200

data = {
    "hour_of_day": np.random.choice(range(8, 22), n),
    "day_of_week": np.random.choice(range(0, 5),  n),
    "is_success":  np.ones(n),
    "ip_octet1":   np.random.choice([192, 193, 10], n),
    "fail_count":  np.random.randint(0, 3, n),
    "ua_hash":     np.random.choice([1, 2, 3], n),
}

df = pd.DataFrame(data)

model = IsolationForest(
    n_estimators=100,
    contamination=0.05,
    random_state=42
)
model.fit(df)

joblib.dump(model,            "model.pkl")
joblib.dump(list(df.columns), "features.pkl")

print("Model u trajnua me 200 login normale")
print("Skedarët e ruajtur: model.pkl, features.pkl")
