(function () {
    const zodiacConstellations = [
        { name: 'Aries', stars: [{x:0.12,y:0.18},{x:0.18,y:0.22},{x:0.22,y:0.20}], lines: [[0,1],[1,2]] },
        { name: 'Taurus', stars: [{x:0.30,y:0.12},{x:0.34,y:0.18},{x:0.38,y:0.14},{x:0.42,y:0.22}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Gemini', stars: [{x:0.55,y:0.10},{x:0.58,y:0.16},{x:0.62,y:0.12},{x:0.64,y:0.20}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Cancer', stars: [{x:0.82,y:0.16},{x:0.85,y:0.22},{x:0.88,y:0.18}], lines: [[0,1],[1,2]] },
        { name: 'Leo', stars: [{x:0.18,y:0.42},{x:0.22,y:0.48},{x:0.24,y:0.42},{x:0.28,y:0.50}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Virgo', stars: [{x:0.41,y:0.36},{x:0.45,y:0.43},{x:0.49,y:0.39},{x:0.52,y:0.46}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Libra', stars: [{x:0.70,y:0.40},{x:0.74,y:0.46},{x:0.78,y:0.42}], lines: [[0,1],[1,2]] },
        { name: 'Scorpio', stars: [{x:0.84,y:0.38},{x:0.88,y:0.44},{x:0.92,y:0.39},{x:0.96,y:0.46}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Sagittarius', stars: [{x:0.15,y:0.75},{x:0.20,y:0.80},{x:0.25,y:0.74},{x:0.30,y:0.82}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Capricorn', stars: [{x:0.40,y:0.68},{x:0.44,y:0.74},{x:0.48,y:0.70}], lines: [[0,1],[1,2]] },
        { name: 'Aquarius', stars: [{x:0.60,y:0.72},{x:0.64,y:0.78},{x:0.68,y:0.74},{x:0.72,y:0.80}], lines: [[0,1],[1,2],[2,3]] },
        { name: 'Pisces', stars: [{x:0.80,y:0.68},{x:0.84,y:0.74},{x:0.88,y:0.70}], lines: [[0,1],[1,2]] }
    ];

    const networkConfig = {
        background: '#05020a',
        connectionColor: 'rgba(164, 60, 255, 0.22)',
        nodeColor: 'rgba(204, 153, 255, 0.90)',
        nodeRadius: 2.5,
        nodeCount: 72,
        connectionDistance: 138,
        speed: 0.35,
    };

    function setupConstellationCanvas() {
        const canvas = document.getElementById('constellationCanvas');
        const parent = document.getElementById('networkContainer');
        if (!canvas || !parent) return;
        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        const resize = () => {
            const dpr = window.devicePixelRatio || 1;
            const width = parent.clientWidth;
            const height = parent.clientHeight;
            canvas.style.width = width + 'px';
            canvas.style.height = height + 'px';
            canvas.width = Math.floor(width * dpr);
            canvas.height = Math.floor(height * dpr);
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            draw();
        };

        const draw = () => {
            const width = parent.clientWidth;
            const height = parent.clientHeight;
            ctx.clearRect(0, 0, width, height);
            ctx.strokeStyle = 'rgba(255,255,255,0.22)';
            ctx.fillStyle = 'rgba(255,255,255,0.9)';
            ctx.lineWidth = 1;
            zodiacConstellations.forEach(constellation => {
                constellation.lines.forEach(([a, b]) => {
                    const from = constellation.stars[a];
                    const to = constellation.stars[b];
                    ctx.beginPath();
                    ctx.moveTo(from.x * width, from.y * height);
                    ctx.lineTo(to.x * width, to.y * height);
                    ctx.stroke();
                });
                constellation.stars.forEach(star => {
                    const x = star.x * width;
                    const y = star.y * height;
                    ctx.beginPath();
                    ctx.arc(x, y, 2.5, 0, Math.PI * 2);
                    ctx.fill();
                });
            });
        };

        window.addEventListener('resize', resize);
        resize();
    }

    function setupNetworkCanvas() {
        const canvas = document.getElementById('networkCanvas');
        const parent = document.getElementById('networkContainer');
        if (!canvas || !parent) return;
        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        let width = 0;
        let height = 0;
        const nodes = [];

        const reset = () => {
            const dpr = window.devicePixelRatio || 1;
            width = parent.clientWidth;
            height = parent.clientHeight;
            canvas.style.width = width + 'px';
            canvas.style.height = height + 'px';
            canvas.width = Math.floor(width * dpr);
            canvas.height = Math.floor(height * dpr);
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            nodes.length = 0;
            for (let i = 0; i < networkConfig.nodeCount; i++) {
                nodes.push({
                    x: Math.random() * width,
                    y: Math.random() * height,
                    vx: (Math.random() - 0.5) * networkConfig.speed,
                    vy: (Math.random() - 0.5) * networkConfig.speed,
                    alpha: 0.5 + Math.random() * 0.5,
                });
            }
        };

        const draw = () => {
            ctx.clearRect(0, 0, width, height);
            ctx.fillStyle = networkConfig.background;
            ctx.fillRect(0, 0, width, height);
            ctx.lineWidth = 0.8;

            for (let i = 0; i < nodes.length; i++) {
                const node = nodes[i];
                for (let j = i + 1; j < nodes.length; j++) {
                    const other = nodes[j];
                    const dx = node.x - other.x;
                    const dy = node.y - other.y;
                    const dist = Math.hypot(dx, dy);
                    if (dist < networkConfig.connectionDistance) {
                        const alpha = ((networkConfig.connectionDistance - dist) / networkConfig.connectionDistance) * 0.18;
                        ctx.strokeStyle = `rgba(164, 60, 255, ${alpha.toFixed(3)})`;
                        ctx.beginPath();
                        ctx.moveTo(node.x, node.y);
                        ctx.lineTo(other.x, other.y);
                        ctx.stroke();
                    }
                }
            }

            nodes.forEach(node => {
                ctx.beginPath();
                const alpha = Math.max(0.35, node.alpha);
                ctx.fillStyle = `rgba(204, 153, 255, ${alpha.toFixed(2)})`;
                ctx.arc(node.x, node.y, networkConfig.nodeRadius, 0, Math.PI * 2);
                ctx.fill();
            });
        };

        const step = () => {
            nodes.forEach(node => {
                node.x += node.vx;
                node.y += node.vy;
                if (node.x <= 0 || node.x >= width) node.vx *= -1;
                if (node.y <= 0 || node.y >= height) node.vy *= -1;
                node.alpha = 0.45 + Math.sin(Date.now() * 0.001 + node.x + node.y) * 0.25;
            });
            draw();
            requestAnimationFrame(step);
        };

        window.addEventListener('resize', reset);
        reset();
        step();
    }

    window.addEventListener('load', () => {
        setupNetworkCanvas();
        setupConstellationCanvas();
    });
})();
