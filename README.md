# Blackjack Wingman

![BJWIN](https://github.com/user-attachments/assets/1bded119-6193-4e2d-b88d-dca601a03105)

## Summary

**Blackjack Wingman** is an educational application designed to teach and train users on how to beat Blackjack. With a variety of interactive trainers, you can master key skills such as **Basic Strategy**, **Card Counting**, **Betting**, and **Deviations**.

The application also includes a **Machine Learning (ML)** environment responsible for deriving a novel _Advanced Strategy_. Utilizing reinforcement learning, the model adapts based on the composition of the shoe, yielding up to a **20% edge over the house**.

## Features

### Free Play

Practice Blackjack freely in a simulated casino environment. Great for casual play or testing strategies in real time.

### Basic Strategy Trainer _(In Progress)_

Learn the mathematically optimal play for each hand. Players will receive real-time feedback and explanations to build foundational knowledge.

### Card Counting Trainer _(In Progress)_

Train your card counting ability using various drills that simulate real-world dealing speeds. Improve your tracking of the running count and conversion to true count.

### Betting Trainer _(In Progress)_

Based on the current True Count, learn how to adjust your betting to maximize expected value and manage risk.

### Deviations Trainer _(In Progress)_

Learn how to use the True Count to make "deviations" from Basic Strategy, enhancing overall playing strategy and adding up to **20% in overall earnings**.

### Game Assessment _(In Progress)_

Evaluate your Blackjack skills through structured tests. Simulate a casino environment to determine whether your strategy, counting, and betting are strong enough to beat the house.

## Machine Learning

### Model

The model is based on **Reinforcement Learning**, using the **Unity ML-Agents** framework and a **Proximal Policy Optimization (PPO)** trainer. It learns by simulating millions of hands and optimizing for long-term rewards.

**Model Input Features:**
- Player Hand Value
- Hand Type (Hard/Soft)
- Dealer Up Card
- True Count (TC)
- Running Count (RC)
- Whether doubling, splitting, or insurance is allowed

**Model Output (Softmax over actions):**
- Hit
- Stand
- Double
- Split
- Take Insurance
- Refuse Insurance

## Results

| True Count | Player Edge (%)      |
|------------|----------------------|
| TC -5      |                      |
| TC -4      |                      |
| TC -3      |                      |
| TC -2      |                      |
| TC -1      |                      |
| TC 0       |                      |
| TC 1       |                      |
| TC 2       |                      |
| TC 3       |                      |
| TC 4       |                      |
| TC 5       |                      |

> *Note: Values will be filled in as testing completes. Preliminary analysis shows up to a 20% edge over the house at high TCs.*

## Disclaimer

**Blackjack Wingman is strictly an educational tool.**

This application is designed to teach users the math and strategy behind Blackjack in a controlled, simulated environment. It is **not** intended to encourage or condone gambling in real casinos.

While this may demonstrate a statistical edge under certain conditions, there is **no guarantee of profit—even with perfect play**. Gambling involves significant financial risk, and this tool should be used for educational purposes only. **Use at your own discretion.**
