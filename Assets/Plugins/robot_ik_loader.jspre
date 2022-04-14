var RobotIK = undefined;
import('./robot_ik_wasm.js').then((result) => {
    console.log(result);
    RobotIK = result;
    RobotIK.default();
});