mergeInto(LibraryManager.library, {
    new_solver : function (urdf, ee_frame) {
        return RobotIK.new_solver(UTF8ToString(urdf), UTF8ToString(ee_frame));
    },

    solve : function (solver_ptr, current_q_ptr, target_ptr, q_ptr) {
        let dof = RobotIK.dof(solver_ptr);

        let current_q = [];
        for(var i = 0; i < dof; i++) {
            current_q.push(HEAPF32[(current_q_ptr >> 2) + i]);
        }

        let target = [];
        for(var k = 0; k < 7; k++) {
            target.push(HEAPF32[(target_ptr >> 2) + k]);
        }

        let result = RobotIK.solve(solver_ptr, current_q, target);

        for (let j = 0; j < dof; j++) {
            HEAPF32[(q_ptr >> 2) + j] = result[j];
        }
        
        // Idk. this is an int stored in a float spot.
        // So ill just check to see if its closer to 0 than > 0.
        return result[dof] < 0.5;
    },

    set_self_collision : function (solver_ptr, enable_self_collision) {
        RobotIK.set_self_collision(solver_ptr, enable_self_collision);
    },

    deallocate : function (solver_ptr) {
        RobotIK.deallocate(solver_ptr);
    }
});