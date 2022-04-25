---
layout: default
title: Robot IK Base
---

# Inverse Kinematics

When controlling a robot, one of the most important aspects to control is the pose(s) (position and orientation) of the end effector(s) in the world frame. In the case of fixed-base robots, these poses are determined by the configurations of the joints. Practically, the geometry of a joint is either *revolute* (one angular degree of freedom), *prismatic* (one linear degree of freedom), or less commonly *spherical* (two angular degrees of freedom). Additionally, robot joint chains can either be *serial*, where the graph of linkages is a tree, or *parallel*, where the graph of linkages contains cycles. Lastly, the base of the robot can either be *fixed* (rigidly coupled to the world frame) or *mobile* (allowed to move). 

To constrain scope of this project, we consider only *fixed-based, single-chain, serial* manipulators with exclusively *revolute* joints[^1]. These kinds of robot arms show up in many flexible operation environments from participating in industrial manufacturing production lines to acting as an extension to a phsyically disabled individual's wheelchair.


## Basic Formulation

The forward kinematics equations for a serial robot arm with $$ n $$ revolutionary joints with angles $$ q_i, 0 \leq i \lt n $$:

$$ {}^nT_0(\vec{q}) = {}^{n}T_{n-1}(q_{n-1}) * ... * {}^{2}T_{1}(q_{1}) * {}^{1}T_{0}(q_{0}) $$


Where the $$ 4x4 $$ homogeneous tranform $$ {}^bT_a $$ changes the frame of a vector from $$ a $$ to $$ b $$ when pre-multiplied. By convention, each joint transform, $$ {}^{i+1}T_{i}(q_{i}) $$, is constructed using the [Denavit–Hartenberg parameters](https://en.wikipedia.org/wiki/Denavit%E2%80%93Hartenberg_parameters) (with or without Craig's convention).

The goal, then is, for some target end-effector pose ($$ {}^{ee}T_0 $$), to find a set of joint angles ($$ \vec{q} $$), such that $$ {}^nT_0(\vec{q}) = {}^{ee}T_0 $$. In general, there are three cases to consider depending on the number of joints in the arm ($$ n $$), and the number of degrees of freedom in the specified pose $$ d $$: under constrained ($$ n \lt d $$), critically constrained($$ n = d $$), and over constrained ($$ n \gt d $$). In the under constrained case, the problem does not have a general solution. In the critically constrained case, it has a finite number of solutions (often several, depending on the symmetries of the arm). And in the under constrained case, there is generally an infinite number of solutions. While it is possible to determine a closed form expression for the finite solutions when $$ n = d $$, the other cases typically employ non-linear optimization to either find a configuration that is close to the desired pose, or select one of the valid solutions. In practice, this same technique is applied in all cases, as it gives the programmer the flexiblity to consider other goals (e.g. obstacle avoidance).

The following interactive widget demonstrates this process on a simple robot with two revolute joints, each with the constriant: $$ -\pi \leq q \leq \pi $$. In the cartesian space representation on the left, the robot will attempt to follow your cursor. On the right, the cost function, $$ f(\vec{q}) = \|\vec{x}(\vec{q}) - \vec{x}_{mouse}\|^2_2 $$, is shown as a heatmap. At a fixed timestep, the joint angles are updated with a simple gradient descent scheme: $$ \vec{q}^{t+1} = \vec{q}^{t} - 0.1 \nabla f(\vec{q}) $$. The yellow trail on the right shows the current and recent values of $$ \vec{q} $$, and the configuration of the robot on the left is updated after each timestep.

# Practical Implementation

## Problem Formulation

In practice, there are a few issues with the gradient descent optimization problem shown above.

Firstly, the convergence rate is often slow. More advanced optimization methods often address this with both adaptive step sizes and more carefully selected step directions. A series of recent papers has yielded practically useful optimization algorithm[^2] and open source library[^3] for solving non-convex optimization problems on embedded, real-time systems. Fundamentally, this approach uses a proximal gradient descent method that is accelerated with the Limited Memory BFGS technique[^4]. In practice, this approach converges in many fewer iterations than standard gradient descent.

Secondly, there are additional constraints that need to be considered. While the stick-bot shown above has no linkage width, a physical arm needs to avoid running into both itself as well as the environment. In this case, the cost function needs to be modified to consider these things. The optimization problem is then posed as:

$$
\begin{align}
    \text{minimize} \qquad & f_{ee}(\vec{q}) + f_{self}(\vec{q}) + f_{env}(\vec{q}) \\
    \text{subject to}       \qquad & lb_i \leq q_i \leq ub_i~\forall{i} 
\end{align}
$$

Where the separate cost terms account for desired end-effector pose and self collision avoidance.

### End Effector Pose

While the straightforward quadratic loss function is not fundamentally incorrect, Rakita et. al. found when developing Relaxed IK that a "groove" loss function around these constraints produced better results[^5]. The parameterizable groove loss function is defined as follows:

$$

groove(x; c, d, f, g) := -e^{-\frac{x^d}{2c^d}} + fx^g

$$

The value of $$ x $$ here is the difference between the current value and the target value. In this context, the pose matching cost is again split into two terms:

$$ 

f_{ee}(\vec{q}) := groove(\|\vec{x_{ee}(\vec{q}) - x_{target}\|, 0.1, 2, 10, 2) + groove(\|R_{target}^{-1}R_{ee}(\vec{q})\|, 0.1, 2, 10, 2)

$$

Where the second term applies the groove loss function to the magnitude of the rotation that takes the current end-effector rotation to the target rotation.

####Self Collision

The self collision cost is inspired by CollisionIK[^6]. For each combination of links, $$ i, j $$, if the links are neither adjacent, nor the same, a cost $$ c_{ij} $$ is added, where 

$$

c_{ij} := \text{dist}(link_i, link_j)^{-2}

$$

Where the function $$ \text{dist}(link_i, link_j) $$ computes the minimum separation distance between the two link geometries. Because this function can become costly to compute if the geometries are complex, a bounding capsule simplification is used. This project **only** considers capsule collision objects. For convenience, all of the relevant robot information is parsed from  a URDF[^7]. If the existing URDF doesn't have capsules, but *does* contain triangle meshes, [this utility](https://github.com/kpwelsh/URDF-Mesh-To-Capsule) can be used to add capsule collision geometries.


[^1]: The end-effector is allowed to have prismatic joints.
[^2]: Stella, L., Themelis, A., Sopasakis, P., & Patrinos, P. (2017, December). A simple and efficient algorithm for nonlinear model predictive control. In 2017 IEEE 56th Annual Conference on Decision and Control (CDC) (pp. 1939-1944). IEEE.
[^3]: Sopasakis, P., Fresk, E., & Patrinos, P. (2020). OpEn: Code generation for embedded nonconvex optimization. IFAC-PapersOnLine, 53(2), 6548-6554.
[^4]: (Limited-memory BFGS)[https://en.wikipedia.org/wiki/Limited-memory_BFGS]
[^5]: Rakita, D., Mutlu, B., & Gleicher, M. (2018, June). RelaxedIK: Real-time Synthesis of Accurate and Feasible Robot Arm Motion. In Robotics: Science and Systems (pp. 26-30).
[^6]: Rakita, D., Shi, H., Mutlu, B., & Gleicher, M. (2021, May). Collisionik: A per-instant pose optimization method for generating robot motions with environment collision avoidance. In 2021 IEEE International Conference on Robotics and Automation (ICRA) (pp. 9995-10001). IEEE.
[^7]: The (URDF spec used by ROS)[http://wiki.ros.org/urdf] is a widely used convention for specifying robot link and joint geometries and dynamics. See the [Robot Arms]({{ site.baseurl }}{% link behind_the_scenes/index.md %}) section of this project for references to specific robot URDFs and models.
[^8]: Don't hate me if something doesn't support the "capsule" primitive in the URDF spec. If you have issues, maybe start looking (here)[https://github.com/ros/urdfdom_headers/pull/24].