# Blackjack Wingman

![BJWIN](https://github.com/user-attachments/assets/1bded119-6193-4e2d-b88d-dca601a03105)

## Summary

**Blackjack Wingman** is a research project exploring algorithmic strategies to beat Blackjack. Using large-scale simulation and reinforcement learning, the project explores various model architectures that maximize long-term expected value under realistic casino rules and shoe dynamics. After countless iterations of prototyping, fine-tuning, and training, a model emerged with a **+18% statistical edge** over the house. 

## Model

The model architecture a **Deep Q-Network (DQN)**, learning from experience and iteratively improving upon itself. DQNs approximate what's known as **Q-values**, which represent the expected future reward of taking an action in a given state. This perfectly aligns with the research objective, as the fundamental goal is to solve a decision problem where each action influences a future reward. 

**Network Architecture**
- **Input**: normalized/encoded observations from environment
- **Hidden Layers**: 2 fully connected layers × **128** neurons each, **ReLU** activations  
- **Output Layer**: **6** nodes (Stand, Hit, Double, Split, Take Insurance, Refuse Insurance) with **Softmax** to produce a distribution over actions (used for action selection).  
- **Loss**: **Mean Squared Error (MSE)** over target Q-values  
- **Optimizer**: **Adam**, initial learning rate **0.0003**

## Training

**Training Hyperparameters**
- **Batch Size**: **1024** (experiences per gradient step)  
- **Replay Buffer Size**: **10,240** (experiences before updates)  
- **Epochs per Update**: **10** (passes over sampled experience)  
- **Discount Factor (γ)**: **0.99**  
- **Normalization**: all inputs normalized/encoded prior to the network

The environment provides rewards derived from hand outcomes; target values are computed from bootstrapped Q-estimates. Action masks ensure the agent only selects legal actions (e.g., Double/Split when permitted).

In addition, to accelerate learning in a relatively large action space and avoid protracted or stalled convergence, reward shaping was introduced during the initial phase of training. Small, shaped signals nudged the agent toward sensible behaviors to establish a competent baseline quickly. After actions stabilized, reward shaping was **annealed and removed**, returning to outcome-based rewards exclusively. This prevented long-term bias from shaped signals and allowed the agent to freely discover potentially more rewarding action sequences under the true objective.

## Results
| Metric           | Hands Simulated | Result  |
|------------------|-----------------|---------|
| Player Edge (EV) | 10,000,000      | **+18%** |
> *Note: Performance metrics may exhibit a degree of variability across different simulations. Nonetheless, the findings were obtained through an impartial and fair simulation.*

## Limitations
While an ML, it's important to understand its limitations.

One limitation the model has is that it's currently limited to a specific set of Blackjack table rules. With various versions of Blackjack being available (), different rules necessitate different strategies. 

## Disclaimer
**Blackjack Wingman strictly represents a research effort.**

This project is designed to explore various strategies with ML models in a controlled and simulated environment. It is **not** intended to encourage or conndone gambling. 

Performance figures are derived from simulations under specific assumptions and **do not guarantee profit** in real-world play. Casinos vary in rules, procedures, and countermeasures. Thus, outcomes in practice can differ substantially.
