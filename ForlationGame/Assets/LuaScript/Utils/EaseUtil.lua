---@class EaseUtil @缓动函数工具
EaseUtil = {};

local _HALF_PI = math.pi * 0.5;
local _PI2 = math.pi * 2;

--- 定义无加速持续运动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.Linear(t, b, c, d)
    return c * t / d + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- 它的运动是类似一个球落向地板又弹起后，几次逐渐减小的回弹运动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.BounceIn(t, b, c, d)
    return c - EaseUtil.BounceOut(d - t, 0, c, d) + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- 它的运动是类似一个球落向地板又弹起后，几次逐渐减小的回弹运动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.BounceInOut(t, b, c, d)
    if (t < d * 0.5) then
        return EaseUtil.BounceIn(t * 2, 0, c, d) * 0.5 + b;
    else
        return EaseUtil.BounceOut(t * 2 - d, 0, c, d) * 0.5 + c * 0.5 + b;
    end
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- 它的运动是类似一个球落向地板又弹起后，几次逐渐减小的回弹运动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.BounceOut(t, b, c, d)
    t = t / d;
    if (t < (1 / 2.75)) then
        return c * (7.5626 * t * t) + b;
    elseif (t < (2 / 2.75)) then
        t = t - (1.5 / 2.75);
        return c * (7.5626 * t * t + 0.75) + b;
    elseif (t < (2.5 / 2.75)) then
        t = t - 2.25 / 2.75;
        return c * (7.5626 * t * t + 0.9375) + b;
    else
        t = t - (2.625 / 2.75);
        return c * (7.5625 * t * t + 0.984375) + b;
    end
end

--- 开始时往后运动，然后反向朝目标移动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param s number 指定过冲量，此处数值越大，过冲越大，不填默认1.70158。
---@return number 指定时间的插补属性的值
function EaseUtil.BackIn(t, b, c, d, s)
    s = s or 1.70158

    t = t / d;

    return c * t * t * ((s + 1) * t - s) + b;
end

--- 开始运动时是向后跟踪，再倒转方向并朝目标移动，稍微过冲目标，然后再次倒转方向，回来朝目标移动。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param s number 指定过冲量，此处数值越大，过冲越大，不填默认1.70158。
---@return number 指定时间的插补属性的值
function EaseUtil.BackInOut(t, b, c, d, s)
    s = s or 1.70158

    t = t / (d * 0.5);

    if (t < 1) then
        s = s * 1.525;
        return c * 0.5 * (t * t * ((s + 1) * t - s)) + b;
    end

    t = t - 2;
    s = s * 1.525;
    return c / 2 * t * t * (((s + 1) * t + s) + 2) + b;
end

--- 开始运动时是朝目标移动，稍微过冲，再倒转方向回来朝着目标。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param s number 指定过冲量，此处数值越大，过冲越大，不填默认1.70158。
---@return number 指定时间的插补属性的值
function EaseUtil.BackOut(t, b, c, d, s)
    s = s or 1.70158

    t = t / d - 1;
    return c * (t * t * ((s + 1) * t + s) + 1) + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- 其中的运动由按照指数方式衰减的正弦波来定义。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param a number 指定正弦波的幅度。
---@param p number 指定正弦波的周期。
---@return number 指定时间的插补属性的值
function EaseUtil.ElasticIn(t, b, c, d, a, p)
    local s;
    if (t == 0) then
        return b;
    end

    t = t / d;
    if (t == 1) then
        return b + c;
    end

    p = p or (d * 0.3);

    if (not a or (c > 0 and a < c) or (c < 0 and a < -c)) then
        a = c;
        s = p / 4;
    else
        s = p / _PI2 * math.asin(c / a);
    end
    t = t - 1;
    return -(a * (2 ^ (10 * t)) * math.sin((t * d - s) * _PI2 / p)) + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- 其中的运动由按照指数方式衰减的正弦波来定义。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param a number 指定正弦波的幅度。
---@param p number 指定正弦波的周期。
---@return number 指定时间的插补属性的值
function EaseUtil.ElasticInOut(t, b, c, d, a, p)
    local s;
    if (t == 0) then
        return b;
    end

    t = t / (d * 0.5);
    if (t == 2) then
        return b + c;
    end
    p = p or (d * (0.3 * 1.5));
    if (not a or (c > 0 and a < c) or (c < 0 and a < -c)) then
        a = c;
        s = p / 4;
    else
        s = p / _PI2 * math.asin(c / a);
    end
    if ( t < 1) then
        t = t - 1;
        return -0.5 * ( a * (2 ^ (10 * t)) * math.sin((t * d - s) * _PI2 / p)) + b;
    end
    t = t - 1
    return a * (2 ^ (-10 * t)) * math.sin((t * d - s) * _PI2 / p) * 0.5 + c + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- 其中的运动由按照指数方式衰减的正弦波来定义。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@param a number 指定正弦波的幅度。
---@param p number 指定正弦波的周期。
---@return number 指定时间的插补属性的值
function EaseUtil.ElasticOut(t, b, c, d, a, p)
    local s;
    if (t == 0) then
        return b;
    end
    t = t / d;
    if (t == 1) then
        return b + c;
    end
    p = p or d * 0.3;
    if (not a or (c > 0 and c < a) or (c < 0 and a < -c)) then
        a = c;
        s = p / 4;
    else
        s = p / _PI2 * math.asin(c / a);
    end
    return a * (2 ^ (-10 * t)) * math.sin((t * d - s) * _PI2 / p) + c + b;
end

--- 以零速率开始运动，然后在执行时加快运动速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.StrongIn(t, b, c, d)
    t = t / d;
    return c * t * t * t * t * t + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.StrongInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * t * t * t * t * t + b;
    end

    t = t - 2;
    return c * 0.5 * (t * t * t * t * t + 2) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.StrongOut(t, b, c, d)
    t = t / d - 1;
    return c * (t * t * t * t * t + 1) + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- Sine 缓动方程中的运动加速度小于 Quad 方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.SineInOut(t, b, c, d)
    return -c * 0.5 * (math.cos(math.pi * t / d) - 1) + b;
end

--- 以零速率开始运动，然后在执行时加快运动速度。
--- Sine 缓动方程中的运动加速度小于 Quad 方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.SineIn(t, b, c, d)
    return -c * math.cos(t / d * _HALF_PI) + c + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- Sine 缓动方程中的运动加速度小于 Quad 方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.SineOut(t, b, c, d)
    return c * math.sin(t / d * _HALF_PI) + b;
end

--- 以零速率开始运动，然后在执行时加快运动速度。
--- Quint 缓动方程的运动加速大于 Quart 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuintIn(t, b, c, d)
    t = t / d;
    return c * t * t * t * t * t + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
---  Quint 缓动方程的运动加速大于 Quart 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuintInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * t * t * t * t * t + b;
    end

    t = t - 2;
    return c * 0.5 * (t * t * t * t * t + 2) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- Quint 缓动方程的运动加速大于 Quart 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuintOut(t, b, c, d)
    t = t / d - 1;
    return c * (t * t * t * t * t + 1) + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- Quart 缓动方程的运动加速大于 Cubic 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuartIn(t, b, c, d)
    t = t / d;
    return c * t * t * t * t + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- Quart 缓动方程的运动加速大于 Cubic 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuartInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * t * t * t * t + b;
    end
    t = t - 2;
    return -c * 0.5 * (t * t * t * t - 2) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- Quart 缓动方程的运动加速大于 Cubic 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuartOut(t, b, c, d)
    t = t / d - 1;
    return -c * (t * t * t * t - 1) + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- Cubic 缓动方程的运动加速大于 Quad 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CubicIn(t, b, c, d)
    t = t / d;
    return c * t * t * t + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- Cubic 缓动方程的运动加速大于 Quad 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CubicInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * t * t * t + b;
    end
    t = t - 2;
    return c * 0.5 * (t * t * t + 2) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- Cubic 缓动方程的运动加速大于 Quad 缓动方程。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CubicOut(t, b, c, d)
    t = t / d - 1;
    return c * (t * t * t + 1) + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- Quad 缓动方程中的运动加速度等于 100% 缓动的时间轴补间的运动加速度，并且显著小于 Cubic 缓动方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuadIn(t, b, c, d)
    t = t / d;
    return c * t * t + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- Quad 缓动方程中的运动加速度等于 100% 缓动的时间轴补间的运动加速度，并且显著小于 Cubic 缓动方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuadInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * t * t + b;
    end

    t = t - 1;
    return -c * 0.5 * (t * (t - 2) - 1) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- Quad 缓动方程中的运动加速度等于 100% 缓动的时间轴补间的运动加速度，并且显著小于 Cubic 缓动方程中的运动加速度。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.QuadOut(t, b, c, d)
    t = t / d;
    return -c * t * (t - 2) + b;
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- 其中每个时间间隔是剩余距离减去一个固定比例部分。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.ExpoIn(t, b, c, d)
    if (t == 0) then
        return b;
    else
        return c * (2 ^ (10 * (t / d - 1))) + b - c * 0.001;
    end
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- 其中每个时间间隔是剩余距离减去一个固定比例部分。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.ExpoInOut(t, b, c, d)
    if (t == 0) then
        return b;
    end
    if (t == d) then
        return b + c;
    end
    t = t / (d * 0.5);
    if (t < 1) then
        return c * 0.5 * (2 ^ (10 * (t - 1))) + b;
    end
    t = t - 1;
    return c * 0.5 * (-(2 ^ (-10 * t)) + 2) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- 其中每个时间间隔是剩余距离减去一个固定比例部分。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.ExpoOut(t, b, c, d)
    if (t == d) then
        return b + c
    else
        return c * (-(2 ^ (-10 * t / d)) + 1) + b;
    end
end

--- 方法以零速率开始运动，然后在执行时加快运动速度。
--- 缓动方程的运动加速会产生突然的速率变化。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CircIn(t, b, c, d)
    t = t / d;
    return -c * (math.sqrt(1 - t * t) - 1) + b;
end

--- 开始运动时速率为零，先对运动进行加速，再减速直到速率为零。
--- 缓动方程的运动加速会产生突然的速率变化。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CircInOut(t, b, c, d)
    t = t / (d * 0.5);
    if (t < 1) then
        return -c * 0.5 * (math.sqrt(1 - t * t) - 1) + b;
    end

    t = t - 2;
    return c * 0.5 * (math.sqrt(1 - t * t) + 1) + b;
end

--- 以较快速度开始运动，然后在执行时减慢运动速度，直至速率为零。
--- 缓动方程的运动加速会产生突然的速率变化。
---@param t number 指定当前时间，介于 0 和持续时间之间（包括二者）。
---@param b number 指定动画属性的初始值。
---@param c number 指定动画属性的更改总计。
---@param d number 指定运动的持续时间。
---@return number 指定时间的插补属性的值
function EaseUtil.CircOut(t, b, c, d)
    t = t / d - 1;
    return c * math.sqrt(1 - t * t) + b;
end