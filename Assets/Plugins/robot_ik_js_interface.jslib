mergeInto(LibraryManager.library, {
    init : function (urdf, dof) {
        return RobotIK.init_ik(UTF8ToString(urdf), dof);
    },

    solve: function (robot_id, dof, current_q_ptr, frame, target_ptr, q_ptr) {
        let current_q = [];
        for(var i = 0; i < dof; i++)
            current_q.push(HEAPF32[(current_q_ptr >> 2) + i]);

        let target = [];
        for(var k = 0; k < 7; k++)
            target.push(HEAPF32[(target_ptr >> 2) + k]);
        let result = RobotIK.solve(robot_id, current_q, UTF8ToString(frame), target);

        for (let j = 0; j < dof; j++) {
            HEAPF32[(q_ptr >> 2) + j] =  result[j];
        }
        return result[dof + 1];
    },
});