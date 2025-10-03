import os
os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'  # Disable tensorflow warning

import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense
import numpy as np
from flask import Flask, request, jsonify
import json
import random

MODEL_NAME = "dummy_v2"

STATE_SIZE = 6
ACTION_SIZE = 2  # Jump or Do Nothing
LEARNING_RATE = 0.0005
DISCOUNT_FACTOR = 0.99  # How much future rewards matter

epsilon = 1.0
MIN_EPSILON = 0.05
DECAY_RATE = 0.9995

step_count = 0
TARGET_UPDATE_FREQUENCY = 2000  # Update target model every N steps
BATCH_SIZE = 32

SAVE_FREQUENCY = 500

def build_model():
    model = Sequential([
        Dense(32, activation="relu", input_shape=(STATE_SIZE,)),
        Dense(16, activation="relu"),
        Dense(ACTION_SIZE, activation="linear")
    ])
    model.compile(optimizer=tf.keras.optimizers.Adam(learning_rate=LEARNING_RATE), loss="mse")
    return model

def train_model(batch_size):
    if len(replay_buffer) < batch_size:
        return
    
    indices = np.random.choice(len(replay_buffer), batch_size, replace=False)
    batch = [replay_buffer[i] for i in indices]

    states = np.array([x[0] for x in batch])
    actions = np.array([x[1] for x in batch])
    rewards = np.array([x[2] for x in batch])
    next_states = np.array([x[3] for x in batch])
    dones = np.array([x[4] for x in batch])

    current_q_values = q_model.predict(states, verbose=0)
    next_q_values = target_model.predict(next_states, verbose=0)
    max_next_q = np.amax(next_q_values, axis=1)

    target_q_values = rewards + DISCOUNT_FACTOR * max_next_q * (1 - dones)

    for i in range(batch_size):
        current_q_values[i, actions[i]] = target_q_values[i]

    q_model.fit(states, current_q_values, epochs=1, verbose=0)


q_model = build_model()
target_model = build_model()

if os.path.exists(f"models/{MODEL_NAME}.weights.h5"):
    print(f"Loading previous weights from {MODEL_NAME}...")
    try:
        q_model.load_weights(f"models/{MODEL_NAME}.weights.h5")
        target_model.set_weights(q_model.get_weights())
        print("Weights loaded successfully.")

        try:
            with open(f"models/{MODEL_NAME}_epsilon.txt", "r") as F:
                epsilon = float(F.read().strip())
                print("Model epsilon loaded successfully.")
        except:
            print("Error loading model epsilon. Starting at 1.0")
        
    except Exception as e:
        print(f"Error loading weights: {e}. Starting fresh.")
else:
    print("No saved weights found. Starting fresh training.")
    target_model.set_weights(q_model.get_weights())

replay_buffer = []
MAX_BUFFER_SIZE = 10000

app = Flask(__name__)
HOST = "0.0.0.0"
PORT = 5000

@app.route("/get_action", methods=["POST"])
def get_action():
    global epsilon
    data = request.json
    state = np.array(data["State"]).reshape(1, STATE_SIZE)

    if np.random.rand() <= epsilon:
        action = random.choice([0, 1])  # Exploration
    else:
        q_values = q_model.predict(state, verbose=0)
        action = np.argmax(q_values[0])  # Use Training Data

    if epsilon > MIN_EPSILON:
        epsilon *= DECAY_RATE
    
    return str(action)

@app.route("/train", methods=["POST"])
def train():
    global step_count
    data = request.json

    s = np.array(data["OldState"])
    a = data["Action"]
    r = data["Reward"]
    ns = np.array(data["NewState"])
    done = data["IsDone"]

    if len(replay_buffer) >= MAX_BUFFER_SIZE:
        replay_buffer.pop(0)
    replay_buffer.append((s, a, r, ns, done))

    if len(replay_buffer) >= BATCH_SIZE and step_count % 4 == 0:
        train_model(BATCH_SIZE)
    
    if step_count > 0 and step_count % TARGET_UPDATE_FREQUENCY == 0:
        target_model.set_weights(q_model.get_weights())
        print(f"--- Target Model Updated at Step {step_count} ---")

    if step_count > 0 and step_count % SAVE_FREQUENCY == 0:
        q_model.save_weights(f"models/{MODEL_NAME}.weights.h5")

        with open(f"models/{MODEL_NAME}_epsilon.txt", "w") as F:
            F.write(str(epsilon))

        print(f"--- Model Weights saved to \"models/{MODEL_NAME}.weights.h5\" ---")
    
    step_count += 1
    return jsonify({"status": "received"})

@app.route("/save", methods=["POST"])
def save():
    q_model.save_weights(f"models/{MODEL_NAME}.weights.h5")

    with open(f"models/{MODEL_NAME}_epsilon.txt", "w") as F:
        F.write(str(epsilon))

    print(f"--- Model Weights saved to \"models/{MODEL_NAME}.weights.h5\" ---")
    return jsonify({"status": "saved"})

if __name__ == "__main__":
    try:
        print(f"Starting Flask server on {HOST}:{PORT}")
        app.run(host=HOST, port=PORT)
    except KeyboardInterrupt:
        print("\nShutting down server. Attempting to save final weights...")
        q_model.save_weights(f"models/{MODEL_NAME}.h5")
        with open(f"models/{MODEL_NAME}_epsilon.txt", "w") as F:
            F.write(str(epsilon))
        print("Final weights saved.")
